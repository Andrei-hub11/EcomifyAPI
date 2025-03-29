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
