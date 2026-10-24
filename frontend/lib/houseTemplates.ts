import { BIMHouseConfig } from '../types';

export interface HouseTemplate {
  id: string;
  name: string;
  subtitle: string;
  description: string;
  image: string;
  bhk: string;
  totalAreaSqm: number;
  config: BIMHouseConfig;
}

export const HOUSE_TEMPLATES: HouseTemplate[] = [
  {
    id: 'l-shaped-deck',
    name: 'L-Shaped Modern Deck Villa',
    subtitle: '3 BHK Contemporary Residence with Alfresco Terrace & Timber Deck',
    description: 'Features a dedicated bedroom wing on the left and a grand open-concept living, dining, and island kitchen area on the right, connecting directly to a covered outdoor dining terrace and sun deck.',
    image: '/templates/template-1-l-shaped-deck.jpg',
    bhk: '3 BHK',
    totalAreaSqm: 145,
    config: {
      floors_count: 1,
      floor_height: 3.1,
      wall_thickness: 0.23,
      quality: 'Premium',
      floors: [
        {
          level: 0,
          height: 3.1,
          rooms: [
            { name: 'Master Suite', type: 'Bedroom', width: 4.8, length: 4.2, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Master Ensuite & Walk-in', type: 'Bathroom', width: 3.0, length: 2.2, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Bedroom 2', type: 'Bedroom', width: 4.2, length: 3.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Bedroom 3', type: 'Bedroom', width: 4.0, length: 3.6, floor_level: 0, floor_material: 'Tiles' },
            { name: 'Common Bath & Laundry', type: 'Bathroom', width: 3.2, length: 2.2, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Great Living Lounge', type: 'LivingRoom', width: 6.8, length: 5.0, floor_level: 0, floor_material: 'Marble' },
            { name: 'Kitchen & Island Bar', type: 'Kitchen', width: 4.6, length: 3.8, floor_level: 0, floor_material: 'Tiles' },
            { name: 'Dining Room', type: 'LivingRoom', width: 4.2, length: 3.5, floor_level: 0, floor_material: 'Marble' },
            { name: 'Covered Alfresco Terrace', type: 'LivingRoom', width: 4.5, length: 3.6, floor_level: 0, floor_material: 'Hardwood' }
          ]
        }
      ]
    }
  },
  {
    id: 'pergola-garden-villa',
    name: 'Luxury Pergola Garden Villa',
    subtitle: '3 BHK Luxury Layout with Private Landscaped Court & Double Balconies',
    description: 'High-end residential design featuring an integrated garden with wooden pergola, dual private balconies, walk-in dressing suites, and a spacious central living hall.',
    image: '/templates/template-2-pergola-garden-villa.jpg',
    bhk: '3 BHK',
    totalAreaSqm: 168,
    config: {
      floors_count: 1,
      floor_height: 3.2,
      wall_thickness: 0.23,
      quality: 'Luxury',
      floors: [
        {
          level: 0,
          height: 3.2,
          rooms: [
            { name: 'Master Retreat & Closet', type: 'Bedroom', width: 5.2, length: 4.4, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Luxury Double Vanity Bath', type: 'Bathroom', width: 3.2, length: 2.5, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Junior Suite with Balcony', type: 'Bedroom', width: 4.2, length: 3.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Bedroom 3 & Study Nook', type: 'Bedroom', width: 4.0, length: 3.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Family Bathroom', type: 'Bathroom', width: 2.4, length: 2.0, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Grand Living Salon', type: 'LivingRoom', width: 7.2, length: 5.2, floor_level: 0, floor_material: 'Marble' },
            { name: 'Chef Kitchen & Breakfast Bar', type: 'Kitchen', width: 5.0, length: 4.2, floor_level: 0, floor_material: 'Tiles' },
            { name: 'Formal Dining Area', type: 'LivingRoom', width: 4.6, length: 3.8, floor_level: 0, floor_material: 'Marble' },
            { name: 'Pergola Garden Court', type: 'LivingRoom', width: 5.2, length: 3.8, floor_level: 0, floor_material: 'Hardwood' }
          ]
        }
      ]
    }
  },
  {
    id: 'wraparound-terrace',
    name: 'Wrap-Around Terrace Villa',
    subtitle: '3 BHK Scandinavian-Style Villa with Continuous Timber Sun Deck',
    description: 'Features a natural wood aesthetic with continuous wrap-around exterior deck, seamless indoor-outdoor sliding glass doors, U-shaped kitchen with utility, and integrated planter perimeter.',
    image: '/templates/template-3-wraparound-terrace.jpg',
    bhk: '3 BHK',
    totalAreaSqm: 152,
    config: {
      floors_count: 1,
      floor_height: 3.0,
      wall_thickness: 0.23,
      quality: 'Premium',
      floors: [
        {
          level: 0,
          height: 3.0,
          rooms: [
            { name: 'Master Suite with Deck View', type: 'Bedroom', width: 4.8, length: 4.2, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Master Ensuite', type: 'Bathroom', width: 2.8, length: 2.2, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Bedroom 2', type: 'Bedroom', width: 4.2, length: 3.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Bedroom 3 / Guest Room', type: 'Bedroom', width: 4.0, length: 3.6, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Central Powder Room', type: 'Bathroom', width: 2.4, length: 1.8, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Open Living Lounge', type: 'LivingRoom', width: 6.2, length: 4.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Dining Hall', type: 'LivingRoom', width: 4.2, length: 3.6, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'U-Shaped Modern Kitchen', type: 'Kitchen', width: 4.5, length: 3.6, floor_level: 0, floor_material: 'Tiles' },
            { name: 'Laundry & Utility', type: 'Office', width: 2.6, length: 2.0, floor_level: 0, floor_material: 'Tiles' }
          ]
        }
      ]
    }
  },
  {
    id: 'central-courtyard-atrium',
    name: 'Central Courtyard / Atrium Villa',
    subtitle: '3 BHK Traditional-Modern Courtyard Home with Central Glass Garden',
    description: 'Built around a bioclimatic glass-enclosed open-to-sky central courtyard with water feature and lush green atrium, offering natural illumination and cross-ventilation to every surrounding room.',
    image: '/templates/template-4-central-courtyard-atrium.jpg',
    bhk: '3 BHK',
    totalAreaSqm: 175,
    config: {
      floors_count: 1,
      floor_height: 3.2,
      wall_thickness: 0.23,
      quality: 'Luxury',
      floors: [
        {
          level: 0,
          height: 3.2,
          rooms: [
            { name: 'Central Garden Atrium', type: 'LivingRoom', width: 4.5, length: 4.5, floor_level: 0, floor_material: 'Tiles' },
            { name: 'Master Suite & Wardrobe', type: 'Bedroom', width: 4.8, length: 4.2, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Luxury Master Bath', type: 'Bathroom', width: 3.0, length: 2.4, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Children Bedroom (Twin Bed)', type: 'Bedroom', width: 4.2, length: 3.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Bedroom 3 (Garden Facing)', type: 'Bedroom', width: 4.0, length: 3.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Guest Bath', type: 'Bathroom', width: 2.2, length: 1.8, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Spacious Living Room', type: 'LivingRoom', width: 6.4, length: 5.0, floor_level: 0, floor_material: 'Marble' },
            { name: 'Dining Room', type: 'LivingRoom', width: 4.5, length: 3.8, floor_level: 0, floor_material: 'Marble' },
            { name: 'Corner Kitchen & Pantry', type: 'Kitchen', width: 4.8, length: 4.0, floor_level: 0, floor_material: 'Tiles' }
          ]
        }
      ]
    }
  },
  {
    id: 'courtyard-open-plan',
    name: 'Atrium Open-Plan Residence',
    subtitle: '3 BHK Open Concept Layout with Central Light Well & Outdoor Terrace',
    description: 'Combines a prominent central green light-well atrium with an expansive open-plan family kitchen, central dining, private bedroom wing, and direct garden terrace connections.',
    image: '/templates/template-5-courtyard-open-plan.jpg',
    bhk: '3 BHK',
    totalAreaSqm: 180,
    config: {
      floors_count: 1,
      floor_height: 3.2,
      wall_thickness: 0.23,
      quality: 'Luxury',
      floors: [
        {
          level: 0,
          height: 3.2,
          rooms: [
            { name: 'Central Landscaped Atrium', type: 'LivingRoom', width: 4.6, length: 4.6, floor_level: 0, floor_material: 'Tiles' },
            { name: 'Master Retreat & Walk-in', type: 'Bedroom', width: 5.0, length: 4.4, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Ensuite Double Bath', type: 'Bathroom', width: 3.2, length: 2.4, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Junior Suite', type: 'Bedroom', width: 4.2, length: 4.0, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Guest Bedroom', type: 'Bedroom', width: 4.2, length: 3.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Family Bath', type: 'Bathroom', width: 2.4, length: 2.0, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Family Living & Entertainment', type: 'LivingRoom', width: 6.6, length: 5.2, floor_level: 0, floor_material: 'Marble' },
            { name: 'Gourmet Kitchen & Island', type: 'Kitchen', width: 5.2, length: 4.2, floor_level: 0, floor_material: 'Tiles' },
            { name: 'Dining Hall', type: 'LivingRoom', width: 4.5, length: 4.0, floor_level: 0, floor_material: 'Marble' }
          ]
        }
      ]
    }
  },
  {
    id: 'duplex-glass-staircase',
    name: 'Modern Glass Staircase Duplex',
    subtitle: '3 BHK 2-Storey Contemporary Duplex with Cantilevered Glass Stairs',
    description: 'An architectural 2-storey duplex featuring a sunken living room, open kitchen, ground guest suite, illuminated glass-railed open-tread staircase, and upper master suite with private sun terrace.',
    image: '/templates/template-6-duplex-glass-staircase.jpg',
    bhk: '3 BHK (2 Storey)',
    totalAreaSqm: 215,
    config: {
      floors_count: 2,
      floor_height: 3.1,
      wall_thickness: 0.23,
      quality: 'Luxury',
      floors: [
        {
          level: 0,
          height: 3.1,
          rooms: [
            { name: 'Sunken Living Lounge', type: 'LivingRoom', width: 6.5, length: 5.0, floor_level: 0, floor_material: 'Marble' },
            { name: 'Island Kitchen & Breakfast Bar', type: 'Kitchen', width: 4.8, length: 4.0, floor_level: 0, floor_material: 'Tiles' },
            { name: 'Dining Atrium', type: 'LivingRoom', width: 4.2, length: 3.8, floor_level: 0, floor_material: 'Marble' },
            { name: 'Ground Guest Suite', type: 'Bedroom', width: 4.2, length: 3.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Ground Ensuite Bath', type: 'Bathroom', width: 2.4, length: 2.0, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Staircase Gallery & Powder Room', type: 'Bathroom', width: 2.6, length: 2.0, floor_level: 0, floor_material: 'Ceramic' }
          ]
        },
        {
          level: 1,
          height: 3.0,
          rooms: [
            { name: 'Upper Master Suite', type: 'Bedroom', width: 5.2, length: 4.5, floor_level: 1, floor_material: 'Hardwood' },
            { name: 'Master Spa Bath & Tub', type: 'Bathroom', width: 3.2, length: 2.5, floor_level: 1, floor_material: 'Ceramic' },
            { name: 'Upper Bedroom 2', type: 'Bedroom', width: 4.2, length: 4.0, floor_level: 1, floor_material: 'Hardwood' },
            { name: 'Upper Bath & Laundry', type: 'Bathroom', width: 2.6, length: 2.2, floor_level: 1, floor_material: 'Ceramic' },
            { name: 'Upper Sun Terrace Balcony', type: 'LivingRoom', width: 4.5, length: 3.0, floor_level: 1, floor_material: 'Hardwood' }
          ]
        }
      ]
    }
  },
  {
    id: 'luxury-pergola-estate',
    name: 'Luxury Pergola Garden Estate',
    subtitle: '3 BHK Single-Level Estate with Landscaped Courtyard & Double Island',
    description: 'Generous executive layout with an integrated pergola terrace, private master dressing room, double-island show kitchen, and a grand sunken living room overlooking the garden.',
    image: '/templates/template-7-luxury-pergola-estate.jpg',
    bhk: '3 BHK',
    totalAreaSqm: 172,
    config: {
      floors_count: 1,
      floor_height: 3.2,
      wall_thickness: 0.23,
      quality: 'Luxury',
      floors: [
        {
          level: 0,
          height: 3.2,
          rooms: [
            { name: 'Master Executive Suite', type: 'Bedroom', width: 5.4, length: 4.4, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Walk-in Dressing & Ensuite', type: 'Bathroom', width: 3.4, length: 2.4, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Bedroom 2 (Garden View)', type: 'Bedroom', width: 4.2, length: 3.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Bedroom 3 (Study Nook)', type: 'Bedroom', width: 4.0, length: 3.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Guest Powder & Bath', type: 'Bathroom', width: 2.4, length: 2.0, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Grand Living Salon', type: 'LivingRoom', width: 7.0, length: 5.2, floor_level: 0, floor_material: 'Marble' },
            { name: 'Gourmet Kitchen & Island', type: 'Kitchen', width: 5.2, length: 4.2, floor_level: 0, floor_material: 'Tiles' },
            { name: 'Formal Dining Area', type: 'LivingRoom', width: 4.6, length: 3.8, floor_level: 0, floor_material: 'Marble' },
            { name: 'Covered Pergola Garden Patio', type: 'LivingRoom', width: 5.0, length: 4.0, floor_level: 0, floor_material: 'Hardwood' }
          ]
        }
      ]
    }
  },
  {
    id: 'split-level-cascading-duplex',
    name: 'Split-Level Cascading Garden Duplex',
    subtitle: '3 BHK 2-Storey Duplex with Vertical Greenery & Floating Mezzanine',
    description: 'An architectural split-level duplex featuring vertical green plant walls, double-height living salon, central internal stairs, glass mezzanine bridge, and upper master suite with private sky balcony.',
    image: '/templates/template-8-split-level-cascading-duplex.jpg',
    bhk: '3 BHK (2 Storey)',
    totalAreaSqm: 230,
    config: {
      floors_count: 2,
      floor_height: 3.2,
      wall_thickness: 0.23,
      quality: 'Luxury',
      floors: [
        {
          level: 0,
          height: 3.2,
          rooms: [
            { name: 'Double-Height Living Salon', type: 'LivingRoom', width: 6.8, length: 5.2, floor_level: 0, floor_material: 'Marble' },
            { name: 'Chef Kitchen & Dining Island', type: 'Kitchen', width: 5.0, length: 4.2, floor_level: 0, floor_material: 'Tiles' },
            { name: 'Ground Guest Suite', type: 'Bedroom', width: 4.2, length: 3.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Ground Ensuite', type: 'Bathroom', width: 2.4, length: 2.0, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Central Staircase Lobby', type: 'LivingRoom', width: 3.5, length: 3.0, floor_level: 0, floor_material: 'Marble' }
          ]
        },
        {
          level: 1,
          height: 3.0,
          rooms: [
            { name: 'Upper Master Suite with Sky Balcony', type: 'Bedroom', width: 5.4, length: 4.5, floor_level: 1, floor_material: 'Hardwood' },
            { name: 'Master Bath & Spa Tub', type: 'Bathroom', width: 3.4, length: 2.5, floor_level: 1, floor_material: 'Ceramic' },
            { name: 'Junior Bedroom with Balcony', type: 'Bedroom', width: 4.4, length: 4.0, floor_level: 1, floor_material: 'Hardwood' },
            { name: 'Upper Bathroom & Laundry', type: 'Bathroom', width: 2.6, length: 2.2, floor_level: 1, floor_material: 'Ceramic' }
          ]
        }
      ]
    }
  },
  {
    id: 'wraparound-deck-lounge',
    name: 'Wrap-Around Deck & Sunken Lounge Villa',
    subtitle: '3 BHK Modern Residence with Planter Deck & Sectional Living',
    description: 'Designed for outdoor-indoor flow with a perimeter planter-lined timber deck, sunken sectional sofa lounge, central open kitchen with cooktop island, and three private bedrooms.',
    image: '/templates/template-9-wraparound-deck-lounge.jpg',
    bhk: '3 BHK',
    totalAreaSqm: 158,
    config: {
      floors_count: 1,
      floor_height: 3.0,
      wall_thickness: 0.23,
      quality: 'Premium',
      floors: [
        {
          level: 0,
          height: 3.0,
          rooms: [
            { name: 'Master Suite with Deck Access', type: 'Bedroom', width: 4.8, length: 4.2, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Master Ensuite & Wardrobe', type: 'Bathroom', width: 3.0, length: 2.2, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Bedroom 2', type: 'Bedroom', width: 4.2, length: 3.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Bedroom 3', type: 'Bedroom', width: 4.0, length: 3.6, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Central Powder & Bath', type: 'Bathroom', width: 2.5, length: 2.0, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Sunken Living Salon', type: 'LivingRoom', width: 6.4, length: 5.0, floor_level: 0, floor_material: 'Marble' },
            { name: 'Island Kitchen & Bar', type: 'Kitchen', width: 4.8, length: 3.8, floor_level: 0, floor_material: 'Tiles' },
            { name: 'Dining Space', type: 'LivingRoom', width: 4.2, length: 3.6, floor_level: 0, floor_material: 'Marble' },
            { name: 'Wrap-Around Planter Sun Deck', type: 'LivingRoom', width: 8.0, length: 3.5, floor_level: 0, floor_material: 'Hardwood' }
          ]
        }
      ]
    }
  },
  {
    id: 'modern-balcony-residence',
    name: 'Modern Balcony & Laundry Suite Residence',
    subtitle: '3 BHK Contemporary Residence with Walk-in Closets & Glass Balcony',
    description: 'Clean modern lines featuring walk-in wardrobes for all three bedrooms, private glass balcony, dedicated laundry and utility suite, open dining hall, and corner kitchen.',
    image: '/templates/template-10-modern-balcony-residence.jpg',
    bhk: '3 BHK',
    totalAreaSqm: 165,
    config: {
      floors_count: 1,
      floor_height: 3.1,
      wall_thickness: 0.23,
      quality: 'Premium',
      floors: [
        {
          level: 0,
          height: 3.1,
          rooms: [
            { name: 'Master Bedroom & Wardrobe', type: 'Bedroom', width: 5.0, length: 4.2, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Master Double Ensuite', type: 'Bathroom', width: 3.0, length: 2.4, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Bedroom 2 with Balcony', type: 'Bedroom', width: 4.4, length: 3.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Bedroom 3 (Wardrobe Suite)', type: 'Bedroom', width: 4.2, length: 3.8, floor_level: 0, floor_material: 'Hardwood' },
            { name: 'Family Powder & Bath', type: 'Bathroom', width: 2.4, length: 2.0, floor_level: 0, floor_material: 'Ceramic' },
            { name: 'Family Living Lounge', type: 'LivingRoom', width: 6.5, length: 5.0, floor_level: 0, floor_material: 'Marble' },
            { name: 'Modern Kitchen & Island', type: 'Kitchen', width: 4.8, length: 3.8, floor_level: 0, floor_material: 'Tiles' },
            { name: 'Open Dining Hall', type: 'LivingRoom', width: 4.4, length: 3.8, floor_level: 0, floor_material: 'Marble' },
            { name: 'Utility & Laundry Room', type: 'Office', width: 2.5, length: 2.2, floor_level: 0, floor_material: 'Tiles' }
          ]
        }
      ]
    }
  }
];
