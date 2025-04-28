INSERT INTO discounts (
    code, discount_type, fixed_amount, percentage,
    max_uses, uses, min_order_amount, max_uses_per_user,
    valid_from, valid_to, is_active, auto_apply
)
VALUES
-- Fixed discounts (type 1)
(NULL, 1, 10.00, NULL, 100, 0, 50.00, 1, now(), now() + INTERVAL '30 days', true, true),
(NULL, 1, 5.00, NULL, 50, 0, 30.00, 1, now(), now() + INTERVAL '15 days', true, false),
(NULL, 1, 20.00, NULL, 200, 0, 100.00, 1, now(), now() + INTERVAL '60 days', true, true),
(NULL, 1, 15.00, NULL, 150, 0, 80.00, 1, now(), now() + INTERVAL '45 days', true, false),
(NULL, 1, 8.00, NULL, 75, 0, 40.00, 1, now(), now() + INTERVAL '20 days', true, true),

-- Percentage discounts (type 2)
(NULL, 2, NULL, 10.00, 100, 0, 50.00, 1, now(), now() + INTERVAL '30 days', true, true),
(NULL, 2, NULL, 5.50, 50, 0, 20.00, 1, now(), now() + INTERVAL '10 days', true, false),
(NULL, 2, NULL, 15.00, 200, 0, 120.00, 1, now(), now() + INTERVAL '60 days', true, true),
(NULL, 2, NULL, 7.25, 120, 0, 70.00, 1, now(), now() + INTERVAL '40 days', true, false),
(NULL, 2, NULL, 20.00, 250, 0, 150.00, 1, now(), now() + INTERVAL '90 days', true, true),

-- Coupon discounts (type 3) — with code and fixed or percentage value
('SAVE10', 3, 10.00, NULL, 100, 0, 60.00, 1, now(), now() + INTERVAL '30 days', true, false),
('OFF5', 3, 5.00, NULL, 50, 0, 25.00, 1, now(), now() + INTERVAL '15 days', true, true),
('HALFOFF', 3, NULL, 50.00, 150, 0, 80.00, 1, now(), now() + INTERVAL '45 days', true, true),
('WELCOME', 3, NULL, 15.00, 200, 0, 100.00, 1, now(), now() + INTERVAL '60 days', true, true),
('EXTRA20', 3, NULL, 20.00, 75, 0, 90.00, 1, now(), now() + INTERVAL '25 days', true, false),
('FIRSTBUY', 3, 12.00, NULL, 80, 0, 70.00, 1, now(), now() + INTERVAL '30 days', true, true),
('SUPER25', 3, NULL, 25.00, 120, 0, 150.00, 1, now(), now() + INTERVAL '50 days', true, false),
('FREESHIP', 3, 7.00, NULL, 90, 0, 40.00, 1, now(), now() + INTERVAL '20 days', true, true),
('SPRINGSALE', 3, NULL, 18.00, 130, 0, 85.00, 1, now(), now() + INTERVAL '40 days', true, false),
('LIMITED50', 3, NULL, 50.00, 70, 0, 120.00, 1, now(), now() + INTERVAL '10 days', true, true);

