SELECT * FROM get_low_stock_products(
    p_stock_threshold := 5,
    p_status := NULL,
    p_name := NULL,
    p_page_size := 5,
    p_page := 1
);
