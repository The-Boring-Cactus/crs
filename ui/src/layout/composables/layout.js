import { computed, reactive, watch, onMounted } from 'vue';

const layoutConfig = reactive({
    preset: localStorage.getItem('layout-preset') || 'Aura',
    primary: localStorage.getItem('layout-primary') || 'emerald',
    surface: localStorage.getItem('layout-surface') || null,
    menuMode: localStorage.getItem('layout-menu-mode') || 'static',
    darkMode: localStorage.getItem('layout-dark-mode') === 'true' || 
              (localStorage.getItem('layout-dark-mode') === null && 
               typeof window !== 'undefined' && 
               window.matchMedia('(prefers-color-scheme: dark)').matches),
    themeColor: localStorage.getItem('layout-theme-color') || 'indigo'
});

const themeColorMap = {
    indigo: {
        primary: '262.1 83.3% 57.8%',
        foreground: '0 0% 100%'
    },
    emerald: {
        primary: '142.1 76.2% 36.3%',
        foreground: '0 0% 100%'
    },
    blue: {
        primary: '221.2 83.2% 53.3%',
        foreground: '0 0% 100%'
    },
    rose: {
        primary: '346.8 77.2% 49.8%',
        foreground: '0 0% 100%'
    },
    amber: {
        primary: '24.6 95% 53.1%',
        foreground: '0 0% 9.5%'
    }
};

const applyTheme = () => {
    if (typeof document === 'undefined') return;
    
    // Apply light/dark class
    const root = document.documentElement;
    if (layoutConfig.darkMode) {
        root.classList.add('dark');
    } else {
        root.classList.remove('dark');
    }
    
    // Apply primary color variables
    const theme = themeColorMap[layoutConfig.themeColor] || themeColorMap.indigo;
    root.style.setProperty('--primary', theme.primary);
    root.style.setProperty('--primary-foreground', theme.foreground);
};

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

    const toggleDarkMode = () => {
        layoutConfig.darkMode = !layoutConfig.darkMode;
    };

    const setThemeColor = (color) => {
        if (themeColorMap[color]) {
            layoutConfig.themeColor = color;
        }
    };

    const isSidebarActive = computed(() => layoutState.overlayMenuActive || layoutState.staticMenuMobileActive);

    const getPrimary = computed(() => layoutConfig.primary);

    const getSurface = computed(() => layoutConfig.surface);

    // Watch for changes and save to localStorage
    watch(
        () => layoutConfig.preset,
        (newValue) => {
            localStorage.setItem('layout-preset', newValue);
        }
    );

    watch(
        () => layoutConfig.primary,
        (newValue) => {
            localStorage.setItem('layout-primary', newValue);
        }
    );

    watch(
        () => layoutConfig.surface,
        (newValue) => {
            localStorage.setItem('layout-surface', newValue || '');
        }
    );

    watch(
        () => layoutConfig.menuMode,
        (newValue) => {
            localStorage.setItem('layout-menu-mode', newValue);
        }
    );

    watch(
        () => layoutConfig.darkMode,
        (newValue) => {
            localStorage.setItem('layout-dark-mode', String(newValue));
            applyTheme();
        }
    );

    watch(
        () => layoutConfig.themeColor,
        (newValue) => {
            localStorage.setItem('layout-theme-color', newValue);
            applyTheme();
        }
    );

    // Initial theme application
    applyTheme();

    return {
        layoutConfig,
        layoutState,
        toggleMenu,
        isSidebarActive,
        getPrimary,
        getSurface,
        setActiveMenuItem,
        toggleDarkMode,
        setThemeColor,
        themeColors: Object.keys(themeColorMap)
    };
}

