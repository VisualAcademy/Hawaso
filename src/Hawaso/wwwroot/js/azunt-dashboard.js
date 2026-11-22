(() => {
  'use strict';

  window.Azunt = window.Azunt || {};
  window.Azunt.shells = window.Azunt.shells || {};

  let activeSidebar = null;
  let activeController = null;
  let activeRefresh = null;

  const init = () => {
      const compactStorageKey = 'azunt.portal.dashboard.sidebar.compact.v2';
      const treeStorageKey = 'azunt.portal.dashboard.tree.classic.v2';
      const shellRoot = document.querySelector('[data-az-shell="dashboard"]');
      const body = shellRoot || document.body;
      const sidebar = shellRoot?.querySelector('#portalSidebar') || document.getElementById('portalSidebar');
      if (!sidebar) return;

      if (activeSidebar === sidebar) {
        // Blazor can rerender its navigation without recreating the layout.
        activeRefresh?.();
        return;
      }
      activeController?.abort();
      activeSidebar = sidebar;
      activeController = new AbortController();
      const { signal } = activeController;
      const topToggle = document.getElementById('sidebarToggle');
      const footerToggle = document.getElementById('sidebarFooterToggle');
      const flyout = document.getElementById('dashboardNavFlyout');
      const flyoutContent = document.getElementById('dashboardNavFlyoutContent');
      const appLauncherToggle = document.getElementById('appLauncherToggle');
      const appLauncherMenu = document.getElementById('appLauncherMenu');
      const searchToggle = sidebar.querySelector('#dashboardNavSearchToggle');
      const collapseAllButton = sidebar.querySelector('#dashboardNavCollapseAll');
      const searchPanel = shellRoot?.querySelector('#dashboardNavSearchPanel') || document.getElementById('dashboardNavSearchPanel');
      const searchCloseButton = searchPanel?.querySelector('#dashboardNavSearchClose');
      const searchInput = searchPanel?.querySelector('#dashboardNavSearchInput');
      const searchResults = searchPanel?.querySelector('#dashboardNavSearchResults');
      const treeState = readTreeState();

      if (localStorage.getItem(compactStorageKey) === 'true') {
        body.classList.add('az-sidebar-collapsed');
      }

      activeRefresh = initializeSourceTree;
      initializeSourceTree();
      syncToggleLabels();

      // Two separate controls:
      // - top hamburger: slide the whole sidebar in/out
      // - footer arrow: expanded sidebar <-> compact icon rail
      topToggle?.addEventListener('click', toggleSidebarVisibility, { signal });
      footerToggle?.addEventListener('click', toggleSidebarCompactness, { signal });
      searchToggle?.addEventListener('click', event => {
        event.preventDefault();
        toggleSearch();
      }, { signal });
      collapseAllButton?.addEventListener('click', event => {
        event.preventDefault();
        collapseAllMenus();
      }, { signal });
      searchCloseButton?.addEventListener('click', () => closeSearch(true), { signal });
      searchInput?.addEventListener('input', renderSearchResults, { signal });
      searchResults?.addEventListener('click', event => {
        if (event.target.closest?.('a.az-dashboard-nav-search-result')) closeSearch();
      }, { signal });
      appLauncherToggle?.addEventListener('click', event => {
        event.preventDefault();
        event.stopPropagation();
        toggleAppLauncher();
      });

      document.addEventListener('click', event => {
        const placeholderLink = event.target.closest?.('[data-placeholder-link]');
        if (placeholderLink && sidebar.contains(placeholderLink)) {
          // Starter-only decorative shortcuts: destination URLs are not configured yet.
          event.preventDefault();
          return;
        }

        if (searchPanel && !searchPanel.hidden &&
            !searchPanel.contains(event.target) && !searchToggle?.contains(event.target)) {
          closeSearch();
        }

        const toggle = event.target.closest?.('[data-nav-toggle]');
        if (toggle) {
          event.preventDefault();
          event.stopPropagation();
          toggleNode(toggle);
          return;
        }

        const link = event.target.closest?.('[data-nav-primary-link]');
        if (link && sidebar?.contains(link) && isCompactMode() && !isSidebarHidden()) {
          const node = link.closest('[data-nav-node]');
          if (node?.dataset.navDepth === '0' && node.dataset.navHasChildren === 'true') {
            event.preventDefault();
            event.stopPropagation();
            openFlyout(node, link);
            return;
          }
        }

        if (flyout?.classList.contains('az-is-open') && !flyout.contains(event.target) && !sidebar?.contains(event.target)) {
          closeFlyout();
        }

        if (appLauncherMenu?.classList.contains('az-is-open') && !appLauncherMenu.contains(event.target) && !appLauncherToggle?.contains(event.target)) {
          closeAppLauncher();
        }
      }, { signal });

      document.addEventListener('keydown', event => {
        if (event.key === 'Escape') {
          closeFlyout();
          closeAppLauncher();
          if (searchPanel && !searchPanel.hidden) closeSearch(true);
        }
      }, { signal });

      window.addEventListener('resize', () => {
        if (!isCompactMode() || isSidebarHidden()) closeFlyout();
      }, { signal });

      function toggleSidebarVisibility() {
        body.classList.toggle('az-sidebar-hidden');
        closeFlyout();
        closeAppLauncher();
        closeSearch();
        syncToggleLabels();
      }

      function toggleSidebarCompactness() {
        const compact = body.classList.toggle('az-sidebar-collapsed');
        closeAppLauncher();
        closeSearch();
        localStorage.setItem(compactStorageKey, String(compact));
        closeFlyout();
        syncToggleLabels();
      }

      function syncToggleLabels() {
        const hidden = isSidebarHidden();
        const compact = body.classList.contains('az-sidebar-collapsed');

        topToggle?.setAttribute('aria-expanded', String(!hidden));
        topToggle?.setAttribute('aria-label', hidden ? 'Show navigation' : 'Hide navigation');
        topToggle?.setAttribute('title', hidden ? 'Show navigation' : 'Hide navigation');

        footerToggle?.setAttribute('aria-label', compact ? 'Expand navigation' : 'Collapse navigation');
        footerToggle?.setAttribute('title', compact ? 'Expand navigation' : 'Collapse navigation');
      }

      function initializeSourceTree() {
        sidebar?.querySelectorAll('[data-nav-toggle]').forEach(button => {
          const node = button.closest('[data-nav-node]');
          if (!node) return;
          const key = node.dataset.navKey;
          const children = directChildren(node);
          if (!key || !children) return;

          // Explicit user choices (including Collapse All) override the active branch.
          if (Object.prototype.hasOwnProperty.call(treeState, key)) {
            setExpanded(node, button, children, treeState[key] === true);
          } else if (node.dataset.activeBranch === 'true') {
            setExpanded(node, button, children, true);
          }
        });
      }

      function toggleNode(button) {
        const node = button.closest('[data-nav-node]');
        const children = node ? directChildren(node) : null;
        if (!node || !children) return;

        const expanded = button.getAttribute('aria-expanded') !== 'true';
        setExpanded(node, button, children, expanded);

        const key = node.dataset.navKey;
        if (key) {
          treeState[key] = expanded;
          localStorage.setItem(treeStorageKey, JSON.stringify(treeState));
          syncSourceNode(key, expanded, node);
        }
      }

      function setExpanded(node, button, children, expanded) {
        node.classList.toggle('az-is-expanded', expanded);
        children.classList.toggle('az-is-collapsed', !expanded);
        button.setAttribute('aria-expanded', String(expanded));
      }

      function directChildren(node) {
        return Array.from(node.children).find(x => x.classList?.contains('dashboard-nav-children')) ?? null;
      }

      function syncSourceNode(key, expanded, origin) {
        sidebar?.querySelectorAll(`[data-nav-node][data-nav-key="${cssEscape(key)}"]`).forEach(node => {
          if (node === origin) return;
          const button = node.querySelector(':scope > .az-dashboard-nav-row > [data-nav-toggle]');
          const children = directChildren(node);
          if (button && children) setExpanded(node, button, children, expanded);
        });
      }

      function openFlyout(sourceNode, trigger) {
        if (!flyout || !flyoutContent) return;

        const clone = sourceNode.cloneNode(true);
        clone.classList.add('az-flyout-root', 'az-is-expanded');
        clone.removeAttribute('id');
        clone.querySelectorAll('[id]').forEach(x => x.removeAttribute('id'));
        clone.querySelectorAll('[aria-controls]').forEach(x => x.removeAttribute('aria-controls'));

        const rootChildren = directChildren(clone);
        if (rootChildren) rootChildren.classList.remove('az-is-collapsed');

        flyoutContent.replaceChildren(clone);
        applyStoredState(clone);

        const rect = trigger.getBoundingClientRect();
        const maxTop = window.innerHeight - 100;
        flyout.style.top = `${Math.min(Math.max(rect.top, 56), maxTop)}px`;
        flyout.classList.add('az-is-open');
        flyout.setAttribute('aria-hidden', 'false');

        requestAnimationFrame(() => {
          const box = flyout.getBoundingClientRect();
          if (box.bottom > window.innerHeight - 8) {
            flyout.style.top = `${Math.max(56, window.innerHeight - box.height - 8)}px`;
          }
        });
      }

      function applyStoredState(root) {
        root.querySelectorAll('[data-nav-node]').forEach(node => {
          if (node.classList.contains('az-flyout-root')) return;
          const button = node.querySelector(':scope > .az-dashboard-nav-row > [data-nav-toggle]');
          const children = directChildren(node);
          if (!button || !children) return;

          const key = node.dataset.navKey;
          const active = node.dataset.activeBranch === 'true';
          const expanded = key && Object.prototype.hasOwnProperty.call(treeState, key) ? treeState[key] === true : active;
          setExpanded(node, button, children, expanded);
        });
      }

      function collapseAllMenus() {
        sidebar.querySelectorAll('[data-nav-node][data-nav-has-children="true"]').forEach(node => {
          const button = node.querySelector(':scope > .az-dashboard-nav-row > [data-nav-toggle]');
          const children = directChildren(node);
          const key = node.dataset.navKey;
          if (!button || !children) return;
          setExpanded(node, button, children, false);
          if (key) treeState[key] = false;
        });
        localStorage.setItem(treeStorageKey, JSON.stringify(treeState));
        closeFlyout();
        closeSearch();
      }

      function toggleSearch() {
        if (!searchPanel || !searchToggle || !searchInput) return;
        if (!searchPanel.hidden) { closeSearch(true); return; }
        closeFlyout();
        closeAppLauncher();
        searchPanel.hidden = false;
        searchToggle.setAttribute('aria-expanded', 'true');
        searchInput.value = '';
        renderSearchResults();
        searchInput.focus();
      }

      function closeSearch(returnFocus = false) {
        if (!searchPanel || searchPanel.hidden) return;
        searchPanel.hidden = true;
        searchToggle?.setAttribute('aria-expanded', 'false');
        if (returnFocus) searchToggle?.focus();
      }

      function renderSearchResults() {
        if (!searchInput || !searchResults) return;
        const query = searchInput.value.trim().toLocaleLowerCase();
        searchResults.replaceChildren();
        if (!query) {
          const hint = document.createElement('div');
          hint.className = 'az-dashboard-nav-search-empty';
          hint.textContent = 'Enter a menu name to search.';
          searchResults.append(hint);
          return;
        }

        // Search the server-rendered navigation only: no hidden/unauthorized
        // entries are imported from the catalog, and placeholders are excluded.
        let count = 0;
        const maxResults = 30;
        sidebar.querySelectorAll('.az-dashboard-nav [data-nav-node]').forEach(node => {
          if (count >= maxResults) return;
          const link = node.querySelector(':scope > .az-dashboard-nav-row > [data-nav-primary-link]');
          if (!link) return;
          const title = link.querySelector('.az-dashboard-nav-text')?.textContent?.trim() || link.textContent.trim();
          const url = link.getAttribute('href');
          if (!url || url === '#') return;
          const crumbs = [];
          let parent = node.parentElement?.closest('[data-nav-node]');
          while (parent) {
            const parentLink = parent.querySelector(':scope > .az-dashboard-nav-row > [data-nav-primary-link]');
            const label = parentLink?.querySelector('.az-dashboard-nav-text')?.textContent?.trim();
            if (label) crumbs.unshift(label);
            parent = parent.parentElement?.closest('[data-nav-node]');
          }
          const path = [...crumbs, title].join(' / ');
          if (!path.toLocaleLowerCase().includes(query)) return;
          const result = document.createElement('a');
          result.className = 'az-dashboard-nav-search-result';
          result.href = url;
          result.textContent = title;
          if (crumbs.length) {
            const subtitle = document.createElement('small');
            subtitle.textContent = crumbs.join(' / ');
            result.append(subtitle);
          }
          searchResults.append(result);
          count++;
        });
        if (count === 0) {
          const empty = document.createElement('div');
          empty.className = 'az-dashboard-nav-search-empty';
          empty.textContent = 'No matching menus.';
          searchResults.append(empty);
        }
      }

      function toggleAppLauncher() {
        if (!appLauncherMenu || !appLauncherToggle) return;

        const open = !appLauncherMenu.classList.contains('az-is-open');
        closeFlyout();
        appLauncherMenu.classList.toggle('az-is-open', open);
        appLauncherMenu.setAttribute('aria-hidden', String(!open));
        appLauncherToggle.classList.toggle('az-is-open', open);
        appLauncherToggle.setAttribute('aria-expanded', String(open));
      }

      function closeAppLauncher() {
        if (!appLauncherMenu || !appLauncherToggle) return;
        appLauncherMenu.classList.remove('az-is-open');
        appLauncherMenu.setAttribute('aria-hidden', 'true');
        appLauncherToggle.classList.remove('az-is-open');
        appLauncherToggle.setAttribute('aria-expanded', 'false');
      }

      function closeFlyout() {
        if (!flyout || !flyoutContent) return;
        flyout.classList.remove('az-is-open');
        flyout.setAttribute('aria-hidden', 'true');
        flyoutContent.replaceChildren();
      }

      function isCompactMode() {
        return body.classList.contains('az-sidebar-collapsed') || window.matchMedia('(max-width: 820px)').matches;
      }

      function isSidebarHidden() {
        return body.classList.contains('az-sidebar-hidden');
      }

      function readTreeState() {
        try {
          const value = JSON.parse(localStorage.getItem(treeStorageKey) ?? '{}');
          return value && typeof value === 'object' ? value : {};
        } catch {
          return {};
        }
      }

      function cssEscape(value) {
        return window.CSS?.escape ? CSS.escape(value) : value.replace(/[^a-zA-Z0-9_-]/g, '\\$&');
      }
  };

  const dispose = () => {
    activeController?.abort();
    activeController = null;
    activeSidebar = null;
    activeRefresh = null;
  };

  window.Azunt.shells.dashboard = window.Azunt.shells.dashboard || {};
  window.Azunt.shells.dashboard.init = init;
  window.Azunt.shells.dashboard.dispose = dispose;

  init();
})();
