-- Drop tables if they exist (in reverse order of dependencies)
DROP TABLE IF EXISTS orders_items;
DROP TABLE IF EXISTS orders;
DROP TABLE IF EXISTS product_categories;
DROP TABLE IF EXISTS products;
DROP TABLE IF EXISTS categories;
DROP TABLE IF EXISTS addresses;
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS discount_history;
DROP TABLE IF EXISTS discount_categories;
DROP TABLE IF EXISTS applied_discounts;
DROP TABLE IF EXISTS discounts;
DROP TABLE IF EXISTS payment_status_history;
DROP TABLE IF EXISTS payment_records;

-- Create tables in order of dependencies

CREATE TYPE user_status AS ENUM ('active', 'inactive', 'deleted');

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    keycloak_id VARCHAR(36) NOT NULL UNIQUE,  -- Maps to Keycloak's user ID
    email VARCHAR(255) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP WITH TIME ZONE,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    profile_picture_url TEXT,
    status user_status NOT NULL DEFAULT 'active',
    CONSTRAINT fk_keycloak_user FOREIGN KEY (keycloak_id) REFERENCES user_entity(id) ON DELETE CASCADE
);

-- User Addresses table
CREATE TABLE user_addresses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_keycloak_id VARCHAR(255) NOT NULL,
    street VARCHAR(255) NOT NULL,
    number int NOT NULL,
    city VARCHAR(100) NOT NULL,
    state VARCHAR(100) NOT NULL,
    zip_code VARCHAR(20) NOT NULL,
    country VARCHAR(100) NOT NULL,
    complement VARCHAR(255),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_user_keycloak FOREIGN KEY (user_keycloak_id) REFERENCES users(keycloak_id) ON DELETE CASCADE
);

-- Categories table
CREATE TABLE categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL UNIQUE,
    description TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Products table
CREATE TABLE products (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    price DECIMAL(18, 2) NOT NULL,
    currency_code VARCHAR(3) NOT NULL,
    stock INT NOT NULL CHECK (stock >= 0),
    image_url VARCHAR(255) NOT NULL,
    status SMALLINT NOT NULL CHECK (status IN (1, 2, 3, 4)),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Product Categories junction table
CREATE TABLE product_categories (
    product_id UUID NOT NULL,
    category_id UUID NOT NULL,
    PRIMARY KEY (product_id, category_id),
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE CASCADE
);

-- Carts table
CREATE TABLE carts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_keycloak_id VARCHAR(255) NOT NULL,
    total_amount DECIMAL(10, 2) NOT NULL,
    currency_code VARCHAR(3) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_user_keycloak FOREIGN KEY (user_keycloak_id) REFERENCES users(keycloak_id) ON DELETE CASCADE
);

-- Cart Items table
CREATE TABLE cart_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cart_id UUID NOT NULL REFERENCES carts(id),
    product_id UUID NOT NULL REFERENCES products(id),
    quantity INT NOT NULL DEFAULT 1,
    unit_price DECIMAL(10, 2) NOT NULL,
    total_price DECIMAL(10, 2) NOT NULL,
    currency_code VARCHAR(3) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_cart_item_cart FOREIGN KEY (cart_id) REFERENCES carts(id) ON DELETE CASCADE,
    CONSTRAINT fk_cart_item_product FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    CONSTRAINT chk_cart_item_total_price CHECK (total_price = unit_price * quantity)
);

-- Orders table
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
    completed_at TIMESTAMP WITHOUT TIME ZONE,
    shipped_at TIMESTAMP WITHOUT TIME ZONE,
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

-- Order Items table
CREATE TABLE order_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL,
    product_id UUID NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(18, 2) NOT NULL,
    currency_code VARCHAR(3) NOT NULL,
    total_price DECIMAL(18, 2) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT order_items_total_price_check CHECK (total_price = unit_price * quantity),
    CONSTRAINT fk_order_items_order FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    CONSTRAINT fk_order_items_product FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

-- Discounts table
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

-- Applied Discounts table
CREATE TABLE applied_discounts (
    cart_id UUID NOT NULL,
    discount_id UUID NOT NULL,
    applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (cart_id, discount_id),
    FOREIGN KEY (cart_id) REFERENCES carts(id),
    FOREIGN KEY (discount_id) REFERENCES discounts(id)
);

-- Discount Categories table
CREATE TABLE discount_categories (
  discount_id UUID NOT NULL REFERENCES discounts(id) ON DELETE CASCADE,
  category_id UUID NOT NULL REFERENCES categories(id) ON DELETE CASCADE,
  PRIMARY KEY (discount_id, category_id)
);

-- Discount History table
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
    CONSTRAINT fk_discount_history_discount FOREIGN KEY (discount_id) REFERENCES discounts(id) ON DELETE CASCADE,

     CONSTRAINT check_discount_values CHECK (
        (discount_type = 1 AND fixed_amount IS NOT NULL AND percentage IS NULL) OR  -- Fixed amount discount
        (discount_type = 2 AND percentage IS NOT NULL AND fixed_amount IS NULL) OR  -- Percentage discount
        (discount_type = 3 AND ((fixed_amount IS NOT NULL OR percentage IS NOT NULL) AND coupon_code IS NOT NULL))  -- Discount with coupon
    )
);

-- Payment Records table
CREATE TABLE payment_records (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL,
    amount DECIMAL(10, 2) NOT NULL,
    currency_code VARCHAR(3) NOT NULL DEFAULT 'BRL',
    payment_method SMALLINT NOT NULL CHECK (payment_method IN (1, 2)),
    transaction_id UUID NOT NULL,
    processed_at TIMESTAMP WITH TIME ZONE NOT NULL,
    status SMALLINT NOT NULL CHECK (status IN (1, 2, 3, 4, 5, 6)),
    gateway_response TEXT,
    
    cc_last_four_digits VARCHAR(4),
    cc_brand VARCHAR(20),
    
    paypal_email VARCHAR(100),
    paypal_payer_id VARCHAR(100),
    
    CONSTRAINT fk_payment_order FOREIGN KEY (order_id) REFERENCES orders(id),
    CONSTRAINT unique_transaction_id UNIQUE (transaction_id)
);

-- Payment Status History table
CREATE TABLE payment_status_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_id UUID NOT NULL,
    status SMALLINT NOT NULL CHECK (status IN (1, 2, 3, 4, 5, 6)),
    timestamp TIMESTAMP WITH TIME ZONE NOT NULL,
    reference TEXT NOT NULL,
    
    CONSTRAINT fk_payment FOREIGN KEY (payment_id) REFERENCES payment_records(id) ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX idx_payment_order_id ON payment_records(order_id);
CREATE INDEX idx_payment_status ON payment_records(status);
CREATE INDEX idx_payment_transaction_id ON payment_records(transaction_id);
CREATE INDEX idx_status_history_payment_id ON payment_status_history(payment_id);

-- Insert initial categories
INSERT INTO categories (name, description) VALUES
    ('Electronics', 'Electronic devices and accessories'),
    ('Clothing', 'Fashion and apparel'),
    ('Books', 'Books and publications'),
    ('Home & Garden', 'Home improvement and garden supplies'),
    ('Sports & Outdoors', 'Sporting goods and outdoor equipment');
