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
    
    CONSTRAINT fk_order FOREIGN KEY (order_id) REFERENCES orders(id),
    CONSTRAINT unique_transaction_id UNIQUE (transaction_id)
);

CREATE TABLE payment_status_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_id UUID NOT NULL,
    status SMALLINT NOT NULL CHECK (status IN (1, 2, 3, 4, 5, 6)),
    timestamp TIMESTAMP WITH TIME ZONE NOT NULL,
    reference TEXT NOT NULL,
    
    CONSTRAINT fk_payment FOREIGN KEY (payment_id) REFERENCES payment_records(id)
);

CREATE INDEX idx_payment_order_id ON payment_records(order_id);
CREATE INDEX idx_payment_status ON payment_records(status);
CREATE INDEX idx_payment_transaction_id ON payment_records(transaction_id);
CREATE INDEX idx_status_history_payment_id ON payment_status_history(payment_id);