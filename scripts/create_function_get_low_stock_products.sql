CREATE OR REPLACE FUNCTION get_low_stock_products(
    p_stock_threshold INTEGER DEFAULT 10,
    p_status SMALLINT DEFAULT NULL,
    p_name TEXT DEFAULT NULL,
    p_page_size INTEGER DEFAULT 10,
    p_page INTEGER DEFAULT 1
)
RETURNS TABLE (
    id UUID,
    name VARCHAR,
    description TEXT,
    price DECIMAL,
    currency_code VARCHAR,
    stock INTEGER,
    image_url VARCHAR,
    status SMALLINT,
    created_at TIMESTAMP,
    updated_at TIMESTAMP,
    total_count BIGINT
)
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        p.id,
        p.name,
        p.description,
        p.price,
        p.currency_code,
        p.stock,
        p.image_url,
        p.status,
        p.created_at,
        p.updated_at,
        COUNT(*) OVER() AS total_count
    FROM products p
    WHERE p.stock <= p_stock_threshold
      AND (p_status IS NULL OR p.status = p_status)
      AND (p_name IS NULL OR p.name ILIKE '%' || p_name || '%')
    ORDER BY p.stock ASC, p.name
    LIMIT p_page_size
    OFFSET (p_page - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;
