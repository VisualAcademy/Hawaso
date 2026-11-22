(() => {
  'use strict';

  window.Azunt = window.Azunt || {};
  window.Azunt.shells = window.Azunt.shells || {};

  let activeHeader = null;
  let activeController = null;
  let legacyMediaCleanup = null;

  const disposeActiveShell = () => {
    const shellRoot = activeHeader?.closest('[data-az-shell="landing"]') || document.querySelector('[data-az-shell="landing"]');

    activeController?.abort();
    activeController = null;
    legacyMediaCleanup?.();
    legacyMediaCleanup = null;
    document.documentElement.classList.remove('az-mobile-nav-open');
    shellRoot?.classList.remove('az-nav-open');
    activeHeader = null;
  };

  const init = () => {
      'use strict';

      const shellRoot = document.querySelector('[data-az-shell="landing"]');
      const header = shellRoot?.querySelector('[data-az-header]') || document.querySelector('[data-az-header]');
      if (!header) {
        disposeActiveShell();
        return;
      }

      if (activeHeader === header) return;
      disposeActiveShell();
      activeHeader = header;
      activeController = new AbortController();
      const { signal } = activeController;

      const primaryNav = header.querySelector('[data-az-primary-nav]');
      const navToggle = header.querySelector('[data-az-nav-toggle]');
      const scrim = header.querySelector('[data-az-menu-scrim]');
      const menuItems = Array.from(header.querySelectorAll('[data-az-menu-item]'));
      const menuTriggers = Array.from(header.querySelectorAll('[data-az-menu-trigger]'));
      const actionMenus = Array.from(header.querySelectorAll('[data-az-action-menu]'));
      const actionTriggers = Array.from(header.querySelectorAll('[data-az-action-trigger]'));
      const desktopMedia = window.matchMedia('(min-width: 961px)');

      const isDesktop = () => desktopMedia.matches;

      const getPanel = trigger => {
        const id = trigger?.getAttribute('aria-controls');
        return id ? document.getElementById(id) : null;
      };

      const viewportGutter = 8;

      // Keep the compact first-level dropdown inside the browser viewport.
      // It opens to the right by default; only edge-align it when that would
      // otherwise create horizontal overflow.
      const fitStandardDropdownToViewport = panel => {
        if (!panel?.classList.contains('az-dropdown') || !isDesktop()) return;

        panel.classList.remove('az-is-edge-aligned');
        panel.style.removeProperty('transform');
        panel.style.maxWidth = `calc(100vw - ${viewportGutter * 2}px)`;

        let rect = panel.getBoundingClientRect();
        if (rect.right > window.innerWidth - viewportGutter) {
          panel.classList.add('az-is-edge-aligned');
          rect = panel.getBoundingClientRect();
        }

        // Defensive clamp for unusually narrow desktop windows or zoom levels.
        if (rect.left < viewportGutter) {
          panel.style.transform = `translateX(${viewportGutter - rect.left}px)`;
        } else if (rect.right > window.innerWidth - viewportGutter) {
          panel.style.transform = `translateX(${window.innerWidth - viewportGutter - rect.right}px)`;
        }
      };

      // Nested dropdowns prefer the familiar right-facing flyout. If the
      // submenu would cross the right edge, flip it to the left. A final clamp
      // prevents any horizontal page scrollbar even at unusual zoom/viewport sizes.
      const fitFlyoutToViewport = details => {
        if (!details || !isDesktop()) return;

        details.classList.remove('az-is-flyout-left');
        const submenu = details.querySelector('.az-dropdown-submenu');
        if (!submenu) return;

        submenu.style.removeProperty('transform');
        submenu.style.maxWidth = `calc(100vw - ${viewportGutter * 2}px)`;

        let rect = submenu.getBoundingClientRect();
        if (rect.right > window.innerWidth - viewportGutter) {
          details.classList.add('az-is-flyout-left');
          rect = submenu.getBoundingClientRect();
        }

        if (rect.left < viewportGutter) {
          submenu.style.transform = `translateX(${viewportGutter - rect.left}px)`;
          rect = submenu.getBoundingClientRect();
        }

        if (rect.right > window.innerWidth - viewportGutter) {
          const current = submenu.style.transform || '';
          const delta = window.innerWidth - viewportGutter - rect.right;
          submenu.style.transform = `${current} translateX(${delta}px)`.trim();
        }
      };

      const resetNestedTabs = item => {
        item?.querySelectorAll('[data-az-tabs]').forEach(tabs => {
          tabs.classList.remove('az-is-mobile-tab-open');
        });

        item?.querySelectorAll('details[data-az-dropdown-nested][open]').forEach(details => {
          details.open = false;
        });
      };

      const closeActionMenu = (menu, restoreFocus = false) => {
        if (!menu) return;

        const trigger = menu.querySelector('[data-az-action-trigger]');
        const panel = trigger ? getPanel(trigger) : null;

        menu.classList.remove('az-is-open');
        trigger?.setAttribute('aria-expanded', 'false');
        if (panel) panel.hidden = true;

        if (restoreFocus) trigger?.focus();
      };

      const closeAllActionMenus = (exceptMenu = null) => {
        actionMenus.forEach(menu => {
          if (menu !== exceptMenu) closeActionMenu(menu);
        });
      };

      const closeMenu = (item, restoreFocus = false) => {
        if (!item) return;

        const trigger = item.querySelector('[data-az-menu-trigger]');
        const panel = trigger ? getPanel(trigger) : null;

        item.classList.remove('az-is-open');
        trigger?.setAttribute('aria-expanded', 'false');
        if (panel) panel.hidden = true;
        resetNestedTabs(item);

        if (restoreFocus) trigger?.focus();
      };

      const syncHeaderState = () => {
        const openItems = menuItems.filter(item => item.classList.contains('az-is-open'));
        const hasOpenMegaMenu = openItems.some(item => item.querySelector('.az-mega:not([hidden])'));
        header.classList.toggle('az-has-open-menu', hasOpenMegaMenu && isDesktop());
        primaryNav?.classList.toggle('az-is-submenu-open', openItems.length > 0 && !isDesktop());
      };

      const closeAllMenus = (exceptItem = null) => {
        menuItems.forEach(item => {
          if (item !== exceptItem) closeMenu(item);
        });
        syncHeaderState();
      };

      const openMenu = item => {
        const trigger = item?.querySelector('[data-az-menu-trigger]');
        const panel = trigger ? getPanel(trigger) : null;
        if (!trigger || !panel) return;

        closeAllActionMenus();
        closeAllMenus(item);
        item.classList.add('az-is-open');
        trigger.setAttribute('aria-expanded', 'true');
        panel.hidden = false;
        fitStandardDropdownToViewport(panel);
        resetNestedTabs(item);
        syncHeaderState();

        if (!isDesktop()) {
          primaryNav?.scrollTo({ top: 0, behavior: 'auto' });
          const back = panel.querySelector('[data-az-menu-back]');
          window.requestAnimationFrame(() => back?.focus({ preventScroll: true }));
        }
      };

      menuTriggers.forEach(trigger => {
        trigger.addEventListener('click', event => {
          event.preventDefault();
          const item = trigger.closest('[data-az-menu-item]');
          const isOpen = item?.classList.contains('az-is-open');

          if (isOpen) {
            closeMenu(item);
            syncHeaderState();
          } else {
            openMenu(item);
          }
        });
      });

      header.querySelectorAll('[data-az-menu-back]').forEach(button => {
        button.addEventListener('click', () => {
          const item = button.closest('[data-az-menu-item]');
          closeMenu(item, true);
          syncHeaderState();
          primaryNav?.scrollTo({ top: 0, behavior: 'auto' });
        });
      });

      const setMobileNav = (open, restoreFocus = false) => {
        if (!primaryNav || !navToggle) return;

        if (open) closeAllActionMenus();
        primaryNav.classList.toggle('az-is-open', open);
        navToggle.setAttribute('aria-expanded', String(open));
        navToggle.setAttribute('aria-label', open ? 'Close navigation' : 'Open navigation');
        primaryNav.setAttribute('aria-hidden', String(!open && !isDesktop()));
        document.documentElement.classList.toggle('az-mobile-nav-open', open);
        (shellRoot || document.body).classList.toggle('az-nav-open', open);

        if (!open) {
          closeAllMenus();
          primaryNav.scrollTop = 0;
          if (restoreFocus) navToggle.focus();
        } else {
          window.requestAnimationFrame(() => {
            const firstTrigger = primaryNav.querySelector('[data-az-menu-trigger]');
            firstTrigger?.focus({ preventScroll: true });
          });
        }
      };

      navToggle?.addEventListener('click', () => {
        setMobileNav(!primaryNav?.classList.contains('az-is-open'));
      });

      actionTriggers.forEach(trigger => {
        trigger.addEventListener('click', event => {
          event.preventDefault();
          event.stopPropagation();

          const menu = trigger.closest('[data-az-action-menu]');
          const panel = getPanel(trigger);
          if (!menu || !panel) return;

          const opening = !menu.classList.contains('az-is-open');
          closeAllActionMenus(menu);

          if (!opening) {
            closeActionMenu(menu);
            return;
          }

          if (!isDesktop() && primaryNav?.classList.contains('az-is-open')) {
            setMobileNav(false);
          }
          closeAllMenus();

          menu.classList.add('az-is-open');
          trigger.setAttribute('aria-expanded', 'true');
          panel.hidden = false;
        });
      });

      header.querySelectorAll('details[data-az-dropdown-nested]').forEach(details => {
        details.addEventListener('toggle', () => {
          details.classList.remove('az-is-flyout-left');
          if (!details.open) return;

          const owner = details.closest('.az-dropdown-list');
          owner?.querySelectorAll('details[data-az-dropdown-nested][open]').forEach(other => {
            if (other !== details) other.open = false;
          });

          window.requestAnimationFrame(() => fitFlyoutToViewport(details));
        });
      });

      scrim?.addEventListener('click', () => closeAllMenus());

      document.addEventListener('click', event => {
        if (!actionMenus.some(menu => menu.contains(event.target))) {
          closeAllActionMenus();
        }

        if (!isDesktop()) return;
        if (!header.contains(event.target)) closeAllMenus();
      }, { signal });

      document.addEventListener('keydown', event => {
        if (event.key !== 'Escape') return;

        const openActionMenu = actionMenus.find(menu => menu.classList.contains('az-is-open'));
        if (openActionMenu) {
          closeActionMenu(openActionMenu, true);
          return;
        }

        const openItem = menuItems.find(item => item.classList.contains('az-is-open'));
        if (openItem) {
          const openTabs = openItem.querySelector('[data-az-tabs].az-is-mobile-tab-open');
          if (!isDesktop() && openTabs) {
            openTabs.classList.remove('az-is-mobile-tab-open');
            openTabs.querySelector('[role="tab"][aria-selected="true"]')?.focus();
            return;
          }

          closeMenu(openItem, true);
          syncHeaderState();
          return;
        }

        if (!isDesktop() && primaryNav?.classList.contains('az-is-open')) {
          setMobileNav(false, true);
        }
      }, { signal });

      const createMobileTabHeading = (panel, menuLabel, tabLabel, tabs) => {
        let heading = panel.querySelector('.az-mobile-tab-heading');
        if (heading) {
          heading.querySelector('strong').textContent = tabLabel;
          return heading;
        }

        heading = document.createElement('div');
        heading.className = 'az-mobile-tab-heading';
        heading.innerHTML = `
          <button type="button" class="az-mobile-tab-back" aria-label="Back to ${menuLabel}">
            <svg aria-hidden="true"><use href="#az-i-back"></use></svg>
            <span>${menuLabel}</span>
          </button>
          <strong>${tabLabel}</strong>`;

        heading.querySelector('.az-mobile-tab-back').addEventListener('click', () => {
          tabs.classList.remove('az-is-mobile-tab-open');
          primaryNav?.scrollTo({ top: 0, behavior: 'auto' });
          tabs.querySelector('[role="tab"][aria-selected="true"]')?.focus({ preventScroll: true });
        });

        panel.prepend(heading);
        return heading;
      };

      header.querySelectorAll('[data-az-tabs]').forEach(tabs => {
        tabs.classList.add('az-tabs-mobile');
        const buttons = Array.from(tabs.querySelectorAll('[role="tab"]'));
        const ownerItem = tabs.closest('[data-az-menu-item]');
        const ownerTrigger = ownerItem?.querySelector('[data-az-menu-trigger]');
        const menuLabel = ownerTrigger?.textContent.trim().replace(/\s+/g, ' ') || 'Menu';

        const activateTab = (button, focus = false, enterMobilePanel = false) => {
          buttons.forEach(tab => {
            const selected = tab === button;
            tab.classList.toggle('az-is-active', selected);
            tab.setAttribute('aria-selected', String(selected));
            tab.tabIndex = selected ? 0 : -1;

            const panelId = tab.getAttribute('aria-controls');
            const panel = panelId ? document.getElementById(panelId) : null;
            if (panel) panel.hidden = !selected;
          });

          const panelId = button.getAttribute('aria-controls');
          const activePanel = panelId ? document.getElementById(panelId) : null;

          if (!isDesktop() && enterMobilePanel && activePanel) {
            createMobileTabHeading(activePanel, menuLabel, button.textContent.trim(), tabs);
            tabs.classList.add('az-is-mobile-tab-open');
            primaryNav?.scrollTo({ top: 0, behavior: 'auto' });
            window.requestAnimationFrame(() => {
              activePanel.querySelector('.az-mobile-tab-back')?.focus({ preventScroll: true });
            });
          }

          if (focus) button.focus();
        };

        buttons.forEach((button, index) => {
          button.addEventListener('click', () => activateTab(button, false, true));
          button.addEventListener('keydown', event => {
            if (!['ArrowRight', 'ArrowLeft', 'ArrowDown', 'ArrowUp', 'Home', 'End', 'Enter', ' '].includes(event.key)) return;

            if ((event.key === 'Enter' || event.key === ' ') && !isDesktop()) {
              event.preventDefault();
              activateTab(button, false, true);
              return;
            }

            if (event.key === 'Enter' || event.key === ' ') return;
            event.preventDefault();

            let nextIndex = index;
            if (event.key === 'Home') nextIndex = 0;
            else if (event.key === 'End') nextIndex = buttons.length - 1;
            else if (event.key === 'ArrowRight' || event.key === 'ArrowDown') nextIndex = (index + 1) % buttons.length;
            else nextIndex = (index - 1 + buttons.length) % buttons.length;

            activateTab(buttons[nextIndex], true, false);
          });
        });
      });

      const syncViewportMode = () => {
        closeAllMenus();
        closeAllActionMenus();
        document.documentElement.classList.remove('az-mobile-nav-open');
        (shellRoot || document.body).classList.remove('az-nav-open');

        if (!primaryNav || !navToggle) return;

        if (isDesktop()) {
          primaryNav.classList.remove('az-is-open');
          primaryNav.removeAttribute('aria-hidden');
          navToggle.setAttribute('aria-expanded', 'false');
          navToggle.setAttribute('aria-label', 'Open navigation');
        } else {
          primaryNav.classList.remove('az-is-open');
          primaryNav.setAttribute('aria-hidden', 'true');
          navToggle.setAttribute('aria-expanded', 'false');
          navToggle.setAttribute('aria-label', 'Open navigation');
        }
      };

      if (typeof desktopMedia.addEventListener === 'function') {
        desktopMedia.addEventListener('change', syncViewportMode, { signal });
      } else {
        desktopMedia.addListener(syncViewportMode);
        legacyMediaCleanup = () => desktopMedia.removeListener(syncViewportMode);
      }

      let resizeFrame = 0;
      window.addEventListener('resize', () => {
        if (!isDesktop()) return;
        window.cancelAnimationFrame(resizeFrame);
        resizeFrame = window.requestAnimationFrame(() => {
          const openItem = menuItems.find(item => item.classList.contains('az-is-open'));
          const openPanel = openItem?.querySelector('.az-dropdown:not([hidden])');
          if (openPanel) fitStandardDropdownToViewport(openPanel);

          openItem?.querySelectorAll('details[data-az-dropdown-nested][open]').forEach(details => {
            fitFlyoutToViewport(details);
          });
        });
      }, { signal });

      syncViewportMode();
  };

  window.Azunt.shells.landing = window.Azunt.shells.landing || {};
  window.Azunt.shells.landing.init = init;
  window.Azunt.shells.landing.dispose = disposeActiveShell;

  init();
})();
