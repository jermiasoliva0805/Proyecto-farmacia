interface ImportMetaEnv {
    readonly VITE_API_BASE_URL: string;
    readonly VITE_APP_NAME: string;
    readonly VITE_USE_COOKIES: string;
    readonly VITE_ORDER_PRINT_PATH?: string;
    readonly VITE_ORDER_LABEL_PATH?: string;
}

interface ImportMeta {
    readonly env: ImportMetaEnv;
}