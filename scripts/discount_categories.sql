CREATE TABLE discount_categories (
  discount_id UUID NOT NULL REFERENCES discounts(id) ON DELETE CASCADE,
  category_id UUID NOT NULL REFERENCES categories(id) ON DELETE CASCADE,
  PRIMARY KEY (discount_id, category_id)
);

