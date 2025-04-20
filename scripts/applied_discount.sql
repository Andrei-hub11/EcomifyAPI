CREATE TABLE applied_discounts (
    cart_id UUID NOT NULL,
    discount_id UUID NOT NULL,
    applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (cart_id, discount_id),
    FOREIGN KEY (cart_id) REFERENCES carts(id),
    FOREIGN KEY (discount_id) REFERENCES discounts(id)
);

