// SignalR and Chat Helper Functions

window.chatHelpers = {
    // Auto-resize textarea based on content
    autoResizeTextArea: function (element) {
        if (!element) return;

        // Reset height to auto to get the correct scrollHeight
        element.style.height = 'auto';

        // Set the height to the scrollHeight
        const maxHeight = 120; // Max height in pixels
        const newHeight = Math.min(element.scrollHeight, maxHeight);
        element.style.height = newHeight + 'px';

        // Add or remove scroll if needed
        element.style.overflowY = element.scrollHeight > maxHeight ? 'auto' : 'hidden';
    },

    // Scroll to bottom of chat messages
    scrollToBottom: function (element, smooth = true) {
        if (!element) return;

        const scrollOptions = {
            top: element.scrollHeight,
            behavior: smooth ? 'smooth' : 'auto'
        };

        element.scrollTo(scrollOptions);
    },

    // Copy text to clipboard
    copyToClipboard: async function (text) {
        try {
            if (navigator.clipboard && navigator.clipboard.writeText) {
                await navigator.clipboard.writeText(text);
                return true;
            } else {
                // Fallback for older browsers
                const textArea = document.createElement('textarea');
                textArea.value = text;
                textArea.style.position = 'fixed';
                textArea.style.opacity = '0';
                document.body.appendChild(textArea);
                textArea.focus();
                textArea.select();

                const successful = document.execCommand('copy');
                document.body.removeChild(textArea);
                return successful;
            }
        } catch (err) {
            console.error('Failed to copy text: ', err);
            return false;
        }
    },

    // Format message timestamp
    formatTimestamp: function (dateString, includeDate = false) {
        const date = new Date(dateString);
        const now = new Date();
        const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
        const messageDate = new Date(date.getFullYear(), date.getMonth(), date.getDate());

        const timeOptions = {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false
        };

        if (includeDate || messageDate < today) {
            const dateOptions = {
                month: 'short',
                day: 'numeric',
                ...timeOptions
            };
            return date.toLocaleDateString('en-US', dateOptions);
        } else {
            return date.toLocaleTimeString('en-US', timeOptions);
        }
    },

    // Debounce function for search/typing indicators
    debounce: function (func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    },

    // Throttle function for scroll events
    throttle: function (func, limit) {
        let inThrottle;
        return function () {
            const args = arguments;
            const context = this;
            if (!inThrottle) {
                func.apply(context, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    },

    // Check if element is in viewport
    isInViewport: function (element) {
        if (!element) return false;

        const rect = element.getBoundingClientRect();
        return (
            rect.top >= 0 &&
            rect.left >= 0 &&
            rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
            rect.right <= (window.innerWidth || document.documentElement.clientWidth)
        );
    },

    // Animate element entrance
    animateElementIn: function (element, animationType = 'fadeIn') {
        if (!element) return;

        element.style.opacity = '0';
        element.style.transform = 'translateY(10px)';
        element.style.transition = 'all 0.3s ease';

        // Trigger animation on next frame
        requestAnimationFrame(() => {
            element.style.opacity = '1';
            element.style.transform = 'translateY(0)';
        });
    },

    // Show typing indicator
    showTypingIndicator: function (container) {
        if (!container) return;

        const existingIndicator = container.querySelector('.typing-indicator');
        if (existingIndicator) return; // Already showing

        const indicator = document.createElement('div');
        indicator.className = 'typing-indicator fade-in';
        indicator.innerHTML = `
            <div class="typing-dots">
                <div class="typing-dot"></div>
                <div class="typing-dot"></div>
                <div class="typing-dot"></div>
            </div>
            <span style="margin-left: 0.5rem; font-size: 0.875rem; color: var(--chat-text-secondary);">
                AI is thinking...
            </span>
        `;

        container.appendChild(indicator);
        this.scrollToBottom(container.parentElement);
    },

    // Hide typing indicator
    hideTypingIndicator: function (container) {
        if (!container) return;

        const indicator = container.querySelector('.typing-indicator');
        if (indicator) {
            indicator.style.opacity = '0';
            indicator.style.transform = 'translateY(-10px)';
            setTimeout(() => {
                indicator.remove();
            }, 200);
        }
    },

    // Handle file upload preview
    previewFile: function (file) {
        return new Promise((resolve, reject) => {
            if (!file) {
                reject('No file provided');
                return;
            }

            const reader = new FileReader();

            reader.onload = function (e) {
                const result = {
                    name: file.name,
                    size: file.size,
                    type: file.type,
                    data: e.target.result
                };
                resolve(result);
            };

            reader.onerror = function () {
                reject('Error reading file');
            };

            // Read file based on type
            if (file.type.startsWith('image/')) {
                reader.readAsDataURL(file);
            } else {
                reader.readAsText(file);
            }
        });
    },

    // Format file size
    formatFileSize: function (bytes) {
        if (bytes === 0) return '0 Bytes';

        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));

        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    },

    // Validate message content
    validateMessage: function (content, maxLength = 2000) {
        if (!content || content.trim().length === 0) {
            return { valid: false, error: 'Message cannot be empty' };
        }

        if (content.length > maxLength) {
            return { valid: false, error: `Message too long (${content.length}/${maxLength} characters)` };
        }

        return { valid: true };
    },

    // Handle keyboard shortcuts
    handleKeyboardShortcuts: function (event, callbacks) {
        const { ctrlKey, metaKey, shiftKey, altKey, key } = event;
        const modifierKey = ctrlKey || metaKey;

        // Ctrl/Cmd + Enter: Send message
        if (modifierKey && key === 'Enter' && callbacks.send) {
            event.preventDefault();
            callbacks.send();
        }

        // Escape: Clear input or close modal
        if (key === 'Escape' && callbacks.escape) {
            event.preventDefault();
            callbacks.escape();
        }

        // Ctrl/Cmd + K: Focus search
        if (modifierKey && key === 'k' && callbacks.search) {
            event.preventDefault();
            callbacks.search();
        }

        // Ctrl/Cmd + /: Show shortcuts
        if (modifierKey && key === '/' && callbacks.help) {
            event.preventDefault();
            callbacks.help();
        }
    },

    // Local storage helpers
    storage: {
        set: function (key, value) {
            try {
                localStorage.setItem(key, JSON.stringify(value));
                return true;
            } catch (error) {
                console.warn('Failed to save to localStorage:', error);
                return false;
            }
        },

        get: function (key, defaultValue = null) {
            try {
                const item = localStorage.getItem(key);
                return item ? JSON.parse(item) : defaultValue;
            } catch (error) {
                console.warn('Failed to read from localStorage:', error);
                return defaultValue;
            }
        },

        remove: function (key) {
            try {
                localStorage.removeItem(key);
                return true;
            } catch (error) {
                console.warn('Failed to remove from localStorage:', error);
                return false;
            }
        },

        clear: function () {
            try {
                localStorage.clear();
                return true;
            } catch (error) {
                console.warn('Failed to clear localStorage:', error);
                return false;
            }
        }
    },

    // Network status detection
    networkStatus: {
        isOnline: function () {
            return navigator.onLine;
        },

        onStatusChange: function (callback) {
            window.addEventListener('online', () => callback(true));
            window.addEventListener('offline', () => callback(false));
        }
    },

    // Performance monitoring
    performance: {
        mark: function (name) {
            if (performance && performance.mark) {
                performance.mark(name);
            }
        },

        measure: function (name, startMark, endMark) {
            if (performance && performance.measure) {
                try {
                    performance.measure(name, startMark, endMark);
                    const measure = performance.getEntriesByName(name)[0];
                    return measure ? measure.duration : 0;
                } catch (error) {
                    console.warn('Performance measurement failed:', error);
                    return 0;
                }
            }
            return 0;
        }
    }
};

// Global utility functions
window.autoResizeTextArea = window.chatHelpers.autoResizeTextArea;
window.scrollToBottom = window.chatHelpers.scrollToBottom;
window.copyToClipboard = window.chatHelpers.copyToClipboard;

// Initialize chat helpers on page load
document.addEventListener('DOMContentLoaded', function () {
    console.log('Chat helpers initialized');

    // Add global keyboard shortcut handling
    document.addEventListener('keydown', function (event) {
        // Only handle shortcuts when not in input fields
        if (event.target.tagName === 'INPUT' || event.target.tagName === 'TEXTAREA') {
            return;
        }

        window.chatHelpers.handleKeyboardShortcuts(event, {
            search: function () {
                const searchInput = document.querySelector('[data-search]');
                if (searchInput) {
                    searchInput.focus();
                }
            },
            help: function () {
                // Could trigger help modal
                console.log('Keyboard shortcuts help');
            }
        });
    });

    // Network status monitoring
    window.chatHelpers.networkStatus.onStatusChange(function (isOnline) {
        const event = new CustomEvent('networkStatusChanged', {
            detail: { isOnline }
        });
        document.dispatchEvent(event);
    });
});

// SignalR connection helpers (if SignalR is used)
window.signalRHelpers = {
    connection: null,

    initialize: function (hubUrl) {
        if (typeof signalR === 'undefined') {
            console.warn('SignalR not loaded');
            return null;
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect([0, 2000, 10000, 30000])
            .build();

        return this.connection;
    },

    start: async function () {
        if (!this.connection) {
            console.error('SignalR connection not initialized');
            return false;
        }

        try {
            await this.connection.start();
            console.log('SignalR connected');
            return true;
        } catch (error) {
            console.error('SignalR connection failed:', error);
            return false;
        }
    },

    stop: async function () {
        if (this.connection) {
            try {
                await this.connection.stop();
                console.log('SignalR disconnected');
                return true;
            } catch (error) {
                console.error('SignalR disconnection failed:', error);
                return false;
            }
        }
        return true;
    },

    on: function (methodName, callback) {
        if (this.connection) {
            this.connection.on(methodName, callback);
        }
    },

    off: function (methodName) {
        if (this.connection) {
            this.connection.off(methodName);
        }
    },

    invoke: async function (methodName, ...args) {
        if (this.connection && this.connection.state === 'Connected') {
            try {
                return await this.connection.invoke(methodName, ...args);
            } catch (error) {
                console.error(`SignalR invoke ${methodName} failed:`, error);
                throw error;
            }
        }
        throw new Error('SignalR not connected');
    }
};