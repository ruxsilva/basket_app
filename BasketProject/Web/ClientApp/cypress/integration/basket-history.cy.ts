describe('Basket History', () => {
  beforeEach(() => {
    cy.login();

    cy.visit('/basket-history');

    cy.get('.loading-container', { timeout: 10000 }).should('not.exist');
  });

  it('should display order history with submitted orders', () => {
    cy.get('.history-header h2').should('contain.text', 'Order History');

    cy.get('.no-history').should('not.exist');

    cy.get('.order-card').should('have.length.at.least', 1);

    cy.get('.order-card').first().within(() => {
      cy.get('.order-date').should('exist');

      cy.get('.order-item').should('have.length.at.least', 1);
      cy.get('.item-name').should('exist');
      cy.get('.item-quantity').should('exist');
      cy.get('.item-price').should('exist');

      cy.get('.order-total').should('exist')
        .and('contain.text', '€');
    });
  });
});
