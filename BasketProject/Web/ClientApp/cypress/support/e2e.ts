import './commands';

Cypress.on('uncaught:exception', (err, runnable) => {
  // Prevent Cypress from failing the test on unhandled exceptions
  return false;
});
