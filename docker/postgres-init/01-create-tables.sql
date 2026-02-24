-- Create the state table with GUID primary key
CREATE TABLE state (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL UNIQUE,
    abbreviation VARCHAR(2) NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for better query performance
CREATE INDEX idx_state_name ON state(name);
CREATE INDEX idx_state_abbreviation ON state(abbreviation);
