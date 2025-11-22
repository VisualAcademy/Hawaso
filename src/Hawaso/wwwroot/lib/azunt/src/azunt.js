// azunt.js - JavaScript Utility Library

// Define the Azunt namespace globally (if it doesn't already exist)
window.Azunt = window.Azunt || {};

// Azunt.TimeZone: Time-related utilities
window.Azunt.TimeZone = {
    /**
     * Returns the local timezone offset in minutes.
     * Example: -540 for Korea Standard Time (UTC+9)
     * @returns {number}
     */
    getLocalOffsetMinutes: function () {
        return new Date().getTimezoneOffset();
    }
};

// Azunt.Text: Text-related utilities
window.Azunt.Text = {
    /**
     * Safely truncates a string containing emojis or surrogate pairs.
     * @param {string} str - The input string to truncate.
     * @param {number} maxLength - The number of visible characters to keep.
     * @param {string} suffix - The suffix to append if truncated (default is "...").
     * @returns {string} The safely truncated string.
     */
    truncateSafe: function (str, maxLength, suffix = "...") {
        if (typeof str !== "string" || maxLength <= 0) return "";

        const chars = Array.from(str); // Handles surrogate pairs
        return chars.length <= maxLength
            ? str
            : chars.slice(0, maxLength).join("") + suffix;
    }
};

// Azunt.Style: CSS utility helpers
window.Azunt.Style = {
    /**
     * Applies single-line ellipsis (...) style to a CSS selector.
     * @param {string} selector - CSS selector to apply the style to.
     * @param {number} maxWidth - Max width in pixels (default: 250).
     */
    applyEllipsis: function (selector, maxWidth = 250) {
        const style = document.createElement("style");
        style.innerHTML = `
            ${selector} {
                max-width: ${maxWidth}px;
                white-space: nowrap;
                overflow: hidden;
                text-overflow: ellipsis;
                display: inline-block;
                vertical-align: middle;
            }
        `;
        document.head.appendChild(style);
    }
};

window.Azunt.UI = {
    /**
     * Truncates the text content of elements matching the selector,
     * safely handles emojis, and applies CSS ellipsis styling.
     *
     * @param {string} selector - CSS selector (e.g., '.title-cell').
     * @param {number} maxTextElements - Max number of visible characters.
     * @param {number} maxWidth - Max pixel width to apply CSS ellipsis.
     * @param {boolean} addTooltip - Whether to set the original text as title.
     */
    applyTruncation: function (selector, maxTextElements = 30, maxWidth = 250, addTooltip = true) {
        // Apply CSS-based ellipsis
        Azunt.Style.applyEllipsis(selector, maxWidth);

        // Select and truncate each matching element
        document.querySelectorAll(selector).forEach(el => {
            const originalText = el.textContent.trim();
            const truncated = Azunt.Text.truncateSafe(originalText, maxTextElements);

            if (truncated !== originalText) {
                el.textContent = truncated;
                if (addTooltip) {
                    el.title = originalText;
                }
            }
        });
    }
};
