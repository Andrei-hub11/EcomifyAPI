CREATE TYPE user_status AS ENUM ('active', 'inactive', 'deleted');

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




