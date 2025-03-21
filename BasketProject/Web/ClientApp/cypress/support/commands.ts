Cypress.Commands.add('login', () => {
  const username = 'test@example.com';
  const password = 'Test123!';

  // Intercept the login request
  cy.intercept('POST', '**/api/auth/login').as('loginRequest');

  // Visit login page
  cy.visit('/login');

  // Wait for the page to load
  cy.get('#email', { timeout: 10000 }).should('be.visible');

  // Enter credentials
  cy.get('#email').clear().type(username);
  cy.get('#password').clear().type(password);

  // Submit login form
  cy.get('button[type="submit"]').click();

  // Wait for login request and verify successful login
  cy.wait('@loginRequest').then((interception) => {
    expect(interception.response?.statusCode).to.eq(200);
  });

  // Verify redirection to basket page
  cy.url({ timeout: 10000 }).should('include', '/basket');

  cy.wait(1000);
});

declare global {
  namespace Cypress {
    interface Chainable {
      login(): Chainable<void>
    }
  }
}

export {};
