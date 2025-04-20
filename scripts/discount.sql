CREATE TABLE discounts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(255) NULL,
    discount_type SMALLINT NOT NULL CHECK (discount_type IN (1, 2, 3)),
    fixed_amount DECIMAL(10, 2) NULL,
    percentage DECIMAL(5, 2) NULL,
    max_uses INT NOT NULL DEFAULT 1,
    uses INT NOT NULL DEFAULT 0,
    min_order_amount DECIMAL(10, 2) NOT NULL DEFAULT 0,
    max_uses_per_user INT NOT NULL DEFAULT 1,
    valid_from TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    valid_to TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    auto_apply BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP

    CONSTRAINT check_discount_values CHECK (
        (discount_type = 1 AND fixed_amount IS NOT NULL AND percentage IS NULL) OR
        (discount_type = 2 AND percentage IS NOT NULL AND fixed_amount IS NULL) OR
        (discount_type = 3 AND ((fixed_amount IS NOT NULL OR percentage IS NOT NULL) AND code IS NOT NULL))
    )
);

CREATE UNIQUE INDEX idx_discount_code_unique 
ON discounts (code) 
WHERE code IS NOT NULL;