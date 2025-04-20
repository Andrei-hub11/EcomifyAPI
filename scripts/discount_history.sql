CREATE TABLE discount_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL,
    customer_id VARCHAR(255) NOT NULL,
    discount_id UUID NOT NULL,
    discount_type SMALLINT NOT NULL CHECK (discount_type IN (1, 2, 3)),
    discount_amount DECIMAL(10, 2) NOT NULL DEFAULT 0,
    percentage DECIMAL(5, 2),
    fixed_amount DECIMAL(10, 2),
    coupon_code VARCHAR(255),
    applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_discount_history_order FOREIGN KEY (order_id) REFERENCES orders(id),
    CONSTRAINT fk_discount_history_customer FOREIGN KEY (customer_id) REFERENCES users(keycloak_id),
    CONSTRAINT fk_discount_history_discount FOREIGN KEY (discount_id) REFERENCES discounts(id),

     CONSTRAINT check_discount_values CHECK (
        (discount_type = 1 AND fixed_amount IS NOT NULL AND percentage IS NULL) OR  -- Fixed amount discount
        (discount_type = 2 AND percentage IS NOT NULL AND fixed_amount IS NULL) OR  -- Percentage discount
        (discount_type = 3 AND ((fixed_amount IS NOT NULL OR percentage IS NOT NULL) AND coupon_code IS NOT NULL))  -- Discount with coupon
    )
);

