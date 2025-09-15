import { computed, reactive, watch } from 'vue';

const layoutConfig = reactive({
    preset: localStorage.getItem('layout-preset') || 'Aura',
    primary: localStorage.getItem('layout-primary') || 'emerald',
    surface: localStorage.getItem('layout-surface') || null,
    menuMode: localStorage.getItem('layout-menu-mode') || 'static',
    darkMode: localStorage.getItem('layout-dark-mode') === 'true'
});

const layoutState = reactive({
    staticMenuDesktopInactive: false,
    overlayMenuActive: false,
    profileSidebarVisible: false,
    configSidebarVisible: false,
    staticMenuMobileActive: false,
    menuHoverActive: false,
    activeMenuItem: null
});

export function useLayout() {
    const setActiveMenuItem = (item) => {
        layoutState.activeMenuItem = item.value || item;
    };

   

    const toggleMenu = () => {
        if (layoutConfig.menuMode === 'overlay') {
            layoutState.overlayMenuActive = !layoutState.overlayMenuActive;
        }

        if (window.innerWidth > 991) {
            layoutState.staticMenuDesktopInactive = !layoutState.staticMenuDesktopInactive;
        } else {
            layoutState.staticMenuMobileActive = !layoutState.staticMenuMobileActive;
        }
    };

    const isSidebarActive = computed(() => layoutState.overlayMenuActive || layoutState.staticMenuMobileActive);

    

    const getPrimary = computed(() => layoutConfig.primary);

    const getSurface = computed(() => layoutConfig.surface);

    const isDarkTheme = computed(() => layoutConfig.darkMode);

    const toggleDarkMode = () => {
        layoutConfig.darkMode = !layoutConfig.darkMode;
        if (layoutConfig.darkMode) {
            document.documentElement.classList.add('app-dark');
        } else {
            document.documentElement.classList.remove('app-dark');
        }
    };

    // Initialize dark mode on startup
    if (layoutConfig.darkMode) {
        document.documentElement.classList.add('app-dark');
    }

    // Watch for changes and save to localStorage
    watch(() => layoutConfig.preset, (newValue) => {
        localStorage.setItem('layout-preset', newValue);
    });

    watch(() => layoutConfig.primary, (newValue) => {
        localStorage.setItem('layout-primary', newValue);
    });

    watch(() => layoutConfig.surface, (newValue) => {
        localStorage.setItem('layout-surface', newValue || '');
    });

    watch(() => layoutConfig.menuMode, (newValue) => {
        localStorage.setItem('layout-menu-mode', newValue);
    });

    watch(() => layoutConfig.darkMode, (newValue) => {
        localStorage.setItem('layout-dark-mode', newValue.toString());
    });

    return {
        layoutConfig,
        layoutState,
        toggleMenu,
        toggleDarkMode,
        isSidebarActive,
        isDarkTheme,
        getPrimary,
        getSurface,
        setActiveMenuItem,
    };
}
