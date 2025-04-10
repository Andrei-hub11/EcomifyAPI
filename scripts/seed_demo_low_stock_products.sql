INSERT INTO categories (name, description)
VALUES 
  ('Livros', 'Livros físicos e e-books'),
  ('Móveis', 'Móveis para casa e escritório'),
  ('Roupas', 'Vestuário e acessórios');

-- Insert products with low stock
INSERT INTO products (name, description, price, currency_code, stock, image_url, status)
VALUES
  ('Smartphone XYZ Pro', 'Smartphone avançado com câmera de alta resolução e processador potente', 1299.99, 'BRL', 3, 'https://example.com/images/smartphone-xyz.jpg', 1),
  ('Notebook UltraSlim', 'Notebook leve e potente para profissionais', 4500.00, 'BRL', 2, 'https://example.com/images/notebook-ultraslim.jpg', 1),
  ('Fones de Ouvido Wireless', 'Fones com cancelamento de ruído e bateria de longa duração', 399.90, 'BRL', 4, 'https://example.com/images/fones-wireless.jpg', 1),
  ('Romance "O Caminho das Nuvens"', 'Best-seller de ficção sobre uma jornada inesperada', 45.90, 'BRL', 1, 'https://example.com/images/livro-caminho-nuvens.jpg', 1),
  ('Manual de Programação Python', 'Guia completo para iniciantes em Python', 89.90, 'BRL', 0, 'https://example.com/images/livro-python.jpg', 2),
  ('Mesa de Escritório Compacta', 'Mesa ideal para espaços pequenos com gavetas', 350.00, 'BRL', 3, 'https://example.com/images/mesa-compacta.jpg', 1),
  ('Cadeira Ergonômica', 'Cadeira ajustável para maior conforto durante o trabalho', 790.00, 'BRL', 1, 'https://example.com/images/cadeira-ergonomica.jpg', 1),
  ('Camiseta Básica Preta', 'Camiseta 100% algodão de alta qualidade', 59.90, 'BRL', 4, 'https://example.com/images/camiseta-preta.jpg', 1),
  ('Jaqueta Impermeável', 'Jaqueta resistente à água para dias chuvosos', 199.90, 'BRL', 2, 'https://example.com/images/jaqueta-impermeavel.jpg', 1),
  ('Tênis Esportivo Runner', 'Tênis leve e confortável para corrida', 289.90, 'BRL', 0, 'https://example.com/images/tenis-runner.jpg', 3),
  ('Livro - O Senhor dos Anéis: As Duas Torres', 'Livro de fantasia do Senhor dos Anéis', 45.90, 'BRL', 2, 'https://example.com/images/livro-senhor-dos-aneis.jpg', 1),
  ('Livro - O Hobbit', 'Livro de fantasia O Hobbit', 40.00, 'BRL', 2, 'https://example.com/images/livro-hobbit.jpg', 1);

-- Relate products to categories
DO $$
DECLARE
  eletronicos_id UUID;
  livros_id UUID;
  moveis_id UUID;
  roupas_id UUID;
BEGIN
  SELECT id INTO eletronicos_id FROM categories WHERE name = 'Eletrônicos';
  SELECT id INTO livros_id FROM categories WHERE name = 'Livros';
  SELECT id INTO moveis_id FROM categories WHERE name = 'Móveis';
  SELECT id INTO roupas_id FROM categories WHERE name = 'Roupas';

  -- Relationships
  INSERT INTO product_categories (product_id, category_id)
  SELECT id, eletronicos_id FROM products WHERE name IN ('Smartphone XYZ Pro', 'Notebook UltraSlim', 'Fones de Ouvido Wireless');
  
  INSERT INTO product_categories (product_id, category_id)
  SELECT id, livros_id FROM products WHERE name IN (
    'Romance "O Caminho das Nuvens"', 
    'Manual de Programação Python',
    'Livro - O Senhor dos Anéis: As Duas Torres',
    'Livro - O Hobbit'
  );

  INSERT INTO product_categories (product_id, category_id)
  SELECT id, moveis_id FROM products WHERE name IN ('Mesa de Escritório Compacta', 'Cadeira Ergonômica');

  INSERT INTO product_categories (product_id, category_id)
  SELECT id, roupas_id FROM products WHERE name IN ('Camiseta Básica Preta', 'Jaqueta Impermeável', 'Tênis Esportivo Runner');
END $$;