import { createRouter, createWebHistory } from 'vue-router';
import { userStoreMe } from '@/store/userStore';
import { useSetupStore } from '@/store/setupStore';

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/setup',
            name: 'setup',
            component: () => import('@/views/pages/SetupWizard.vue'),
            meta: { requiresAuth: false, isSetup: true }
        },
        {
            path: '/',
            name: 'home',
            component: () => import('@/views/Index.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/sqleditor',
            name: 'sqleditor',
            component: () => import('@/views/pages/SqlEditor.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/myexcel',
            name: 'myexcel',
            component: () => import('@/views/pages/MyExcel.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/databases',
            name: 'databases',
            component: () => import('@/views/pages/Databases.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/cseditor',
            name: 'cseditor',
            component: () => import('@/views/pages/CsEditor.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/dashboard',
            name: 'dashboard',
            component: () => import('@/views/pages/Dashboard.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/empty',
            name: 'empty',
            component: () => import('@/views/pages/Empty.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/crud',
            name: 'crud',
            component: () => import('@/views/pages/Crud.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/documentation',
            name: 'documentation',
            component: () => import('@/views/pages/Documentation.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/charts',
            name: 'charts',
            component: () => import('@/views/pages/ChartDemo.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/notfound',
            name: 'notfound',
            component: () => import('@/views/pages/NotFound.vue')
        },

        {
            path: '/pages/reports',
            name: 'reports',
            component: () => import('@/views/pages/Reports.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/share/:shareToken',
            name: 'public-view',
            component: () => import('@/views/pages/PublicView.vue'),
            meta: { requiresAuth: false }
        },
        {
            path: '/public/:shareToken',
            redirect: to => ({ name: 'public-view', params: { shareToken: to.params.shareToken } })
        },
        {
            path: '/auth/login',
            name: 'login',
            redirect: '/'
        },
        {
            path: '/auth/access',
            name: 'accessDenied',
            component: () => import('@/views/pages/auth/Access.vue')
        },
        {
            path: '/auth/error',
            name: 'error',
            component: () => import('@/views/pages/auth/Error.vue')
        }
    ]
});

let setupChecked = false;
let isConfigured = null;

router.beforeEach(async (to, from, next) => {
    const userStore = userStoreMe();

    // Check setup status once on first navigation
    if (!setupChecked) {
        try {
            const setupStore = useSetupStore();
            isConfigured = await setupStore.checkStatus();
            setupChecked = true;
        } catch (e) {
            console.error('Failed to check setup status:', e);
            isConfigured = false;
            setupChecked = true;
        }
    }

    // If not configured and not going to setup page → redirect to setup
    if (!isConfigured && to.name !== 'setup') {
        next({ name: 'setup' });
        return;
    }

    // If configured and trying to go to setup → redirect to login or home
    if (isConfigured && to.name === 'setup') {
        if (userStore.auth) {
            next({ name: 'home' });
        } else {
            next({ name: 'login' });
        }
        return;
    }

    // Normal auth guard
    const requiresAuth = to.matched.some((record) => record.meta.requiresAuth);
    if (requiresAuth && !userStore.auth) {
        // App.vue handles showing the login screen
        next();
    } else {
        next();
    }
});

export default router;
