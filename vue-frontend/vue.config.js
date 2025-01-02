const { defineConfig } = require('@vue/cli-service');

module.exports = defineConfig({
    transpileDependencies: true,
    devServer: {
        proxy: {
            '/api': {
                target: 'https://localhost:7190', // Backend URL
                changeOrigin: true, // Ensure the host header is changed to the target URL
                secure: false, // Disable SSL verification for development
            },
        },
    },
});
