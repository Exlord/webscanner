import type { UserConfig } from 'vite';

export default {
  build: {
    rollupOptions: {
      output: {
        manualChunks: function manualChunks(id) {
          if (id.includes('index.html') || id.includes('src/main.ts')) {
            return 'test';
          }

          return 'exlord.web.scanner';
        },
      },
    },
  },
} satisfies UserConfig;
