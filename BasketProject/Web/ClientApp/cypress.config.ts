import { defineConfig } from 'cypress';

let userData = null;

export default defineConfig({
  e2e: {
    baseUrl: 'http://localhost:8081',
    supportFile: 'cypress/support/e2e.ts',
    specPattern: 'cypress/integration/**/*.cy.ts',
    env: {
      apiUrl: 'http://localhost:8081/api'
    },
    setupNodeEvents(on, config) {
      on('task', {
        setUserData(user) {
          userData = user;
          return null;
        },
        getUserData() {
          return userData || null;
        },
      });
    }
  },

  component: {
    devServer: {
      framework: 'angular',
      bundler: 'webpack'
    }
  }
});
