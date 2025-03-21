describe('Basket Form', () => {
  beforeEach(() => {
    cy.login()

    cy.get('.loading-container', { timeout: 10000 }).should('not.exist');
  });

  it('should load and display available items', () => {
    cy.get('.item-card', { timeout: 10000 }).should('have.length.at.least', 1);
    cy.get('.item-name').should('be.visible');
    cy.get('.item-price').should('be.visible');
  });

  it('should increment and decrement item quantities', () => {
    cy.get('.item-card').first().within(() => {
      cy.get('.quantity-display').should('have.value', '0');

      cy.get('button mat-icon').contains('add').parent().click().click().click();
      cy.get('.quantity-display').should('have.value', '3');

      cy.get('.item-subtotal').should('be.visible');

      cy.get('button mat-icon').contains('remove').parent().click().click();
      cy.get('.quantity-display').should('have.value', '1');
    });
  });

  it('should handle manual quantity input', () => {
    cy.get('.item-card').first().within(() => {
      cy.get('.quantity-display').clear().type('5', { force: true }).blur();

      cy.get('.quantity-display').should('have.value', '50');

      cy.get('.item-subtotal').should('be.visible');
    });
  });

  it('should display basket summary when items are added', () => {
    cy.get('.basket-summary').should('not.exist');

    cy.get('.item-card').first().within(() => {
      cy.get('button mat-icon').contains('add').parent().click().click();
    });

    cy.get('.basket-summary', { timeout:
        5000 }).should('be.visible');
    cy.get('.selected-items .selected-item', { timeout: 5000 }).should('have.length.at.least', 1);
    cy.get('.basket-total').should('be.visible');
  });

  it('should update basket total when multiple items are added', () => {
    cy.get('.item-card').eq(0).within(() => {
      cy.get('button mat-icon').contains('add').parent().click().click();
    });

    cy.get('.item-card').eq(1).within(() => {
      cy.get('button mat-icon').contains('add').parent().click().click().click();
    });

    cy.get('.selected-items .selected-item', { timeout: 5000 }).should('have.length.at.least', 2);

    let basketTotal;
    cy.get('.basket-total-row span:last-child', { timeout: 5000 }).invoke('text').then(text => {
      basketTotal = text;
    });

    cy.get('.item-card').eq(0).within(() => {
      cy.get('button mat-icon').contains('remove').parent().click();
    });

    cy.wait(500);
    cy.get('.basket-total-row span:last-child').invoke('text').should('not.eq', basketTotal);
  });

  it('should disable submit button when basket is empty', () => {
    cy.get('.item-card').each(($card) => {
      cy.wrap($card).within(() => {
        cy.get('.quantity-display').invoke('val').then(val => {
          const quantity = parseInt(val as string);
          if (quantity > 0) {
            cy.get('.quantity-display').clear().type('0', { force: true }).blur();
          }
        });
      });
    });

    cy.get('button[type="submit"]').should('be.disabled');
  });

  it('should submit order and display receipt', () => {
    // Add items to the basket
    cy.get('.item-card').first().within(() => {
      cy.get('button mat-icon').contains('add').parent().click().click();
    });

    cy.get('.item-card').eq(1).within(() => {
      cy.get('button mat-icon').contains('add').parent().click();
    });

    // Submit button should be enabled
    cy.get('button[type="submit"]', { timeout: 5000 }).should('be.enabled');

    // Submit the order
    cy.get('button[type="submit"]').click();

    // Check for any loading indicator
    cy.get('button[type="submit"]').then($button => {
      const isDisabled = $button.prop('disabled');
      expect(isDisabled, 'Submit button should be disabled during processing').to.be.true;
    });

    // Wait for receipt to appear
    cy.get('app-receipt-display', { timeout: 15000 }).should('be.visible');

    // Verify receipt has the expected elements based on your HTML structure
    cy.get('.receipt-container').should('be.visible');
    cy.get('.receipt-title').should('contain.text', 'Receipt');

    // Check for items section
    cy.get('.items-section').should('exist');
    cy.get('.receipt-item').should('have.length.at.least', 1);

    // Check for expected item details
    cy.get('.item-name').should('exist');
    cy.get('.item-quantity').should('exist');
    cy.get('.item-price').should('exist');

    // Check for totals section
    cy.get('.totals-section').should('exist');
    cy.get('.final-total').should('exist')
      .within(() => {
        cy.contains('Total:').should('exist');
        cy.contains('€').should('exist');
      });

    // Basket should be reset after successful submission
    cy.wait(1000); // Small delay to allow UI to update
    cy.get('.basket-summary').should('not.exist');
    cy.get('.item-card').first().within(() => {
      cy.get('.quantity-display').should('have.value', '0');
    });
  });

  it('should handle API errors when loading items', () => {
    cy.intercept('GET', '**/items', {
      statusCode: 500,
      body: 'Server error'
    }).as('getItemsFailure');

    cy.visit('/basket');

    cy.wait('@getItemsFailure');

    cy.contains(/error|failed|unable/i, { timeout: 5000 }).should('be.visible');
  });

  it('should validate quantities cannot be negative', () => {
    cy.get('.item-card').first().within(() => {
      cy.get('.quantity-display').clear().type('-5', { force: true }).blur();

      cy.get('.quantity-display').invoke('val').then(val => {
        const value = parseInt(val as string);
        expect(value).to.be.at.least(0);
      });
    });
  });

  it('should calculate basket totals correctly', () => {
    cy.get('.item-card').eq(0).within(() => {
      cy.get('.quantity-display').clear().type('2', { force: true }).blur();
    });

    cy.get('.item-card').eq(1).within(() => {
      cy.get('.quantity-display').clear().type('3', { force: true }).blur();
    });

    cy.wait(1000);

    cy.get('.basket-summary', { timeout: 5000 }).should('be.visible');

    cy.get('.selected-items .selected-item').should('have.length.at.least', 2);

    cy.get('.basket-total-row span:last-child')
      .invoke('text')
      .should('match', /€\d+(\.\d{2})?/);

    cy.get('.basket-total-row span:last-child')
      .invoke('text')
      .then(text => {
        const value = parseFloat(text.replace('€', ''));
        expect(value).to.be.gt(0);
      });
  });
});
