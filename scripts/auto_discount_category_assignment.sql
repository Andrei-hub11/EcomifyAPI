WITH auto_discounts AS (
    SELECT id AS discount_id
    FROM discounts
    WHERE 
        auto_apply = TRUE
        AND is_active = TRUE
        AND valid_from <= NOW()
        AND valid_to >= NOW()
),

target_categories AS (
    SELECT id AS category_id
    FROM categories
    WHERE name IN ('Eletrônicos', 'Celulares')
)

INSERT INTO discount_categories (discount_id, category_id)
SELECT 
    ad.discount_id,
    tc.category_id
FROM auto_discounts ad
CROSS JOIN target_categories tc
ON CONFLICT DO NOTHING;
