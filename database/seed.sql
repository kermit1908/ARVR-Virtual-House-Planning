-- Reference Construction Material and Labour Rates
-- Based on CPWD / State PWD DSR (Delhi Schedule of Rates) & Market Reference Averages
-- Disclaimer: Reference rates used for preliminary BIM cost estimation.

DELETE FROM material_rates;

-- 1. Bengaluru (Karnataka)
INSERT INTO material_rates (id, location, category, item_name, unit, unit_rate) VALUES
('blr_c1', 'Bengaluru', 'Concrete', 'Ready Mix RCC Concrete (M25)', 'm3', 5200.00),
('blr_s1', 'Bengaluru', 'Steel', 'TMT Steel Reinforcement (Fe500D)', 'kg', 72.00),
('blr_b1', 'Bengaluru', 'Masonry', 'Standard Red Clay Bricks', 'piece', 11.50),
('blr_b2', 'Bengaluru', 'Masonry', 'AAC Lightweight Blocks', 'm3', 3800.00),
('blr_m1', 'Bengaluru', 'Cement', 'Portland Pozzolana Cement (50kg)', 'bag', 390.00),
('blr_m2', 'Bengaluru', 'Sand', 'Manufactured Sand (M-Sand)', 'm3', 1450.00),
('blr_f1', 'Bengaluru', 'Flooring', 'Vitrified Floor Tiles (600x600)', 'm2', 850.00),
('blr_f2', 'Bengaluru', 'Flooring', 'Granite Stone Flooring', 'm2', 1750.00),
('blr_p1', 'Bengaluru', 'Finishing', 'Internal Acrylic Emulsion Paint (2 coats)', 'm2', 220.00),
('blr_p2', 'Bengaluru', 'Finishing', 'External Weather-Proof Paint', 'm2', 280.00),
('blr_d1', 'Bengaluru', 'Openings', 'Main Door (Teak Finish Flush Door)', 'unit', 14500.00),
('blr_d2', 'Bengaluru', 'Openings', 'Internal Bedroom/Bathroom Door', 'unit', 7500.00),
('blr_w1', 'Bengaluru', 'Openings', 'UPVC Sliding Glazed Window', 'm2', 4200.00),
('blr_l1', 'Bengaluru', 'Labour', 'Civil Construction Labour', 'm2_builtup', 3800.00),
('blr_sv1', 'Bengaluru', 'Services', 'Electrical & Plumbing Rough-in & Fixtures', 'm2_builtup', 1850.00);

-- 2. Chennai (Tamil Nadu)
INSERT INTO material_rates (id, location, category, item_name, unit, unit_rate) VALUES
('che_c1', 'Chennai', 'Concrete', 'Ready Mix RCC Concrete (M25)', 'm3', 5050.00),
('che_s1', 'Chennai', 'Steel', 'TMT Steel Reinforcement (Fe500D)', 'kg', 71.00),
('che_b1', 'Chennai', 'Masonry', 'Standard Red Clay Bricks', 'piece', 10.80),
('che_b2', 'Chennai', 'Masonry', 'AAC Lightweight Blocks', 'm3', 3650.00),
('che_m1', 'Chennai', 'Cement', 'Portland Pozzolana Cement (50kg)', 'bag', 380.00),
('che_m2', 'Chennai', 'Sand', 'Manufactured Sand (M-Sand)', 'm3', 1500.00),
('che_f1', 'Chennai', 'Flooring', 'Vitrified Floor Tiles (600x600)', 'm2', 820.00),
('che_f2', 'Chennai', 'Flooring', 'Granite Stone Flooring', 'm2', 1650.00),
('che_p1', 'Chennai', 'Finishing', 'Internal Acrylic Emulsion Paint (2 coats)', 'm2', 210.00),
('che_p2', 'Chennai', 'Finishing', 'External Weather-Proof Paint', 'm2', 270.00),
('che_d1', 'Chennai', 'Openings', 'Main Door (Teak Finish Flush Door)', 'unit', 13800.00),
('che_d2', 'Chennai', 'Openings', 'Internal Bedroom/Bathroom Door', 'unit', 7200.00),
('che_w1', 'Chennai', 'Openings', 'UPVC Sliding Glazed Window', 'm2', 4000.00),
('che_l1', 'Chennai', 'Labour', 'Civil Construction Labour', 'm2_builtup', 3650.00),
('che_sv1', 'Chennai', 'Services', 'Electrical & Plumbing Rough-in & Fixtures', 'm2_builtup', 1750.00);

-- 3. Hyderabad (Telangana)
INSERT INTO material_rates (id, location, category, item_name, unit, unit_rate) VALUES
('hyd_c1', 'Hyderabad', 'Concrete', 'Ready Mix RCC Concrete (M25)', 'm3', 4900.00),
('hyd_s1', 'Hyderabad', 'Steel', 'TMT Steel Reinforcement (Fe500D)', 'kg', 70.00),
('hyd_b1', 'Hyderabad', 'Masonry', 'Standard Red Clay Bricks', 'piece', 10.20),
('hyd_b2', 'Hyderabad', 'Masonry', 'AAC Lightweight Blocks', 'm3', 3550.00),
('hyd_m1', 'Hyderabad', 'Cement', 'Portland Pozzolana Cement (50kg)', 'bag', 370.00),
('hyd_m2', 'Hyderabad', 'Sand', 'Manufactured Sand (M-Sand)', 'm3', 1380.00),
('hyd_f1', 'Hyderabad', 'Flooring', 'Vitrified Floor Tiles (600x600)', 'm2', 800.00),
('hyd_f2', 'Hyderabad', 'Flooring', 'Granite Stone Flooring', 'm2', 1600.00),
('hyd_p1', 'Hyderabad', 'Finishing', 'Internal Acrylic Emulsion Paint (2 coats)', 'm2', 205.00),
('hyd_p2', 'Hyderabad', 'Finishing', 'External Weather-Proof Paint', 'm2', 265.00),
('hyd_d1', 'Hyderabad', 'Openings', 'Main Door (Teak Finish Flush Door)', 'unit', 13500.00),
('hyd_d2', 'Hyderabad', 'Openings', 'Internal Bedroom/Bathroom Door', 'unit', 7000.00),
('hyd_w1', 'Hyderabad', 'Openings', 'UPVC Sliding Glazed Window', 'm2', 3900.00),
('hyd_l1', 'Hyderabad', 'Labour', 'Civil Construction Labour', 'm2_builtup', 3500.00),
('hyd_sv1', 'Hyderabad', 'Services', 'Electrical & Plumbing Rough-in & Fixtures', 'm2_builtup', 1700.00);

-- 4. Delhi NCR
INSERT INTO material_rates (id, location, category, item_name, unit, unit_rate) VALUES
('del_c1', 'Delhi', 'Concrete', 'Ready Mix RCC Concrete (M25)', 'm3', 5350.00),
('del_s1', 'Delhi', 'Steel', 'TMT Steel Reinforcement (Fe500D)', 'kg', 74.50),
('del_b1', 'Delhi', 'Masonry', 'Standard Red Clay Bricks', 'piece', 12.00),
('del_b2', 'Delhi', 'Masonry', 'AAC Lightweight Blocks', 'm3', 3950.00),
('del_m1', 'Delhi', 'Cement', 'Portland Pozzolana Cement (50kg)', 'bag', 410.00),
('del_m2', 'Delhi', 'Sand', 'Manufactured Sand (M-Sand)', 'm3', 1600.00),
('del_f1', 'Delhi', 'Flooring', 'Vitrified Floor Tiles (600x600)', 'm2', 880.00),
('del_f2', 'Delhi', 'Flooring', 'Granite Stone Flooring', 'm2', 1850.00),
('del_p1', 'Delhi', 'Finishing', 'Internal Acrylic Emulsion Paint (2 coats)', 'm2', 230.00),
('del_p2', 'Delhi', 'Finishing', 'External Weather-Proof Paint', 'm2', 290.00),
('del_d1', 'Delhi', 'Openings', 'Main Door (Teak Finish Flush Door)', 'unit', 15000.00),
('del_d2', 'Delhi', 'Openings', 'Internal Bedroom/Bathroom Door', 'unit', 7800.00),
('del_w1', 'Delhi', 'Openings', 'UPVC Sliding Glazed Window', 'm2', 4350.00),
('del_l1', 'Delhi', 'Labour', 'Civil Construction Labour', 'm2_builtup', 3950.00),
('del_sv1', 'Delhi', 'Services', 'Electrical & Plumbing Rough-in & Fixtures', 'm2_builtup', 1900.00);

-- 5. Mumbai (Maharashtra)
INSERT INTO material_rates (id, location, category, item_name, unit, unit_rate) VALUES
('mum_c1', 'Mumbai', 'Concrete', 'Ready Mix RCC Concrete (M25)', 'm3', 5600.00),
('mum_s1', 'Mumbai', 'Steel', 'TMT Steel Reinforcement (Fe500D)', 'kg', 76.00),
('mum_b1', 'Mumbai', 'Masonry', 'Standard Red Clay Bricks', 'piece', 12.50),
('mum_b2', 'Mumbai', 'Masonry', 'AAC Lightweight Blocks', 'm3', 4100.00),
('mum_m1', 'Mumbai', 'Cement', 'Portland Pozzolana Cement (50kg)', 'bag', 425.00),
('mum_m2', 'Mumbai', 'Sand', 'Manufactured Sand (M-Sand)', 'm3', 1750.00),
('mum_f1', 'Mumbai', 'Flooring', 'Vitrified Floor Tiles (600x600)', 'm2', 920.00),
('mum_f2', 'Mumbai', 'Flooring', 'Granite Stone Flooring', 'm2', 1950.00),
('mum_p1', 'Mumbai', 'Finishing', 'Internal Acrylic Emulsion Paint (2 coats)', 'm2', 245.00),
('mum_p2', 'Mumbai', 'Finishing', 'External Weather-Proof Paint', 'm2', 310.00),
('mum_d1', 'Mumbai', 'Openings', 'Main Door (Teak Finish Flush Door)', 'unit', 15800.00),
('mum_d2', 'Mumbai', 'Openings', 'Internal Bedroom/Bathroom Door', 'unit', 8200.00),
('mum_w1', 'Mumbai', 'Openings', 'UPVC Sliding Glazed Window', 'm2', 4600.00),
('mum_l1', 'Mumbai', 'Labour', 'Civil Construction Labour', 'm2_builtup', 4200.00),
('mum_sv1', 'Mumbai', 'Services', 'Electrical & Plumbing Rough-in & Fixtures', 'm2_builtup', 2100.00);
