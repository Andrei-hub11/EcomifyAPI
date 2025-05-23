CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_keycloak_id VARCHAR(36) NOT NULL,
    total_amount DECIMAL(18,2) NOT NULL,
    discount_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    total_with_discount DECIMAL(18,2) NOT NULL DEFAULT 0,
    currency_code VARCHAR(3) NOT NULL,
    order_date TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    status SMALLINT NOT NULL CHECK (status IN (1, 2, 3, 4, 5)),
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    shipped_at TIMESTAMP WITHOUT TIME ZONE,
    completed_at TIMESTAMP WITHOUT TIME ZONE,
    shipping_street VARCHAR(255) NOT NULL,
    shipping_number INT NOT NULL,
    shipping_city VARCHAR(100) NOT NULL,
    shipping_state VARCHAR(100) NOT NULL,
    shipping_zip_code VARCHAR(20) NOT NULL,
    shipping_country VARCHAR(100) NOT NULL,
    shipping_complement VARCHAR(255),
    billing_street VARCHAR(255) NOT NULL,
    billing_number INT NOT NULL,
    billing_city VARCHAR(100) NOT NULL,
    billing_state VARCHAR(100) NOT NULL,
    billing_zip_code VARCHAR(20) NOT NULL,
    billing_country VARCHAR(100) NOT NULL,
    billing_complement VARCHAR(255),
    CONSTRAINT fk_user_keycloak FOREIGN KEY (user_keycloak_id) REFERENCES users(keycloak_id) ON DELETE CASCADE
);
