describe('Authentication Flow', () => {
  const timestamp = new Date().getTime();
  const testUser = {
    email: `test${timestamp}@example.com`,
    password: 'Test123!'
  };

  // Save the user credentials to Cypress environment for other tests to use
  before(() => {
    cy.wrap(testUser).as('testUser');
    cy.task('setUserData', testUser);
  });
  beforeEach(() => {
    cy.visit('/register');
  });

  it('should successfully register a new user', () => {
    cy.get('#email').type(testUser.email);
    cy.get('#password').type(testUser.password);
    cy.get('#confirmPassword').type(testUser.password);

    cy.get('button[type="submit"]').click();

    cy.url().should('include', '/basket');
    cy.get('.navbar-menu .user-icon').should('be.visible');
  });

  it('should show validation errors for invalid registration', () => {
    cy.get('#email').type('invalid-email').blur();
    cy.contains('Please enter a valid email address').should('be.visible');

    cy.get('#password').type('short').blur();
    cy.contains('Password must be at least 6 characters').should('be.visible');

    cy.get('#confirmPassword')
      .type('different')
      .blur();

    cy.get('h2').click();

    cy.contains('Please enter a valid email address').should('be.visible');
    cy.contains('Password must be at least 6 characters').should('be.visible');
    cy.contains('Passwords do not match').should('be.visible');

    cy.get('button[type="submit"]')
      .should('be.disabled')
      .and('have.class', 'btn-primary');
  });

  it('should login with newly registered user', () => {
    cy.visit('/login');

    cy.get('#email').type(testUser.email);
    cy.get('#password').type(testUser.password);

    cy.get('button[type="submit"]').click();

    cy.url().should('include', '/basket');
    cy.get('.navbar-menu .user-icon').should('be.visible');
  });

  it('should show error for invalid login credentials', () => {
    cy.visit('/login');

    cy.get('#email').type('nonexistent@example.com');
    cy.get('#password').type('wrongpassword');

    cy.get('button[type="submit"]').click();

    cy.get('.alert-danger').should('be.visible')
      .and('contain', 'Invalid email or password');
  });

  it('should prevent duplicate registration', () => {
    cy.get('#email').type(testUser.email);
    cy.get('#password').type(testUser.password);
    cy.get('#confirmPassword').type(testUser.password);

    cy.get('button[type="submit"]').click();

    cy.get('.alert-danger').should('be.visible')
      .and('contain', 'Email already exists');
  });
});
