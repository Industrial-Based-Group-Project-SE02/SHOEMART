import { pool } from "../db.js";

function validateProductName(name) {
  // Name: 2-100 characters
  return name && name.trim().length >= 2 && name.trim().length <= 100;
}

function validatePrice(price) {
  // Price: must be a positive number
  return price !== null && price !== undefined && !isNaN(price) && Number(price) > 0;
}

function validateStock(stock) {
  // Stock: must be 0 or positive integer
  return stock !== null && stock !== undefined && !isNaN(stock) && Number(stock) >= 0 && Number.isInteger(Number(stock));
}

function validateSizeValue(size_value) {
  // Size: non-empty string, max 20 chars (e.g., "S", "M", "L", "XL", "42", "10.5")
  return size_value && size_value.trim().length > 0 && size_value.trim().length <= 20;
}

function validateColor(color) {
  // Color: 2-30 characters, letters and spaces only
  if (!color) return true; // optional field
  const colorRegex = /^[a-zA-Z\s]{2,30}$/;
  return colorRegex.test(color);
}

function validateCountry(country) {
  // Country: 2-50 characters
  if (!country) return true; // optional field
  return country.trim().length >= 2 && country.trim().length <= 50;
}

function validateImages(images) {
  // Images: must be array of valid URLs (if provided)
  if (!images) return true; // optional
  if (!Array.isArray(images)) return false;
  
  const urlRegex = /^(https?:\/\/)/i;
  for (const img of images) {
    if (typeof img !== "string" || !urlRegex.test(img)) {
      return false;
    }
  }
  return true;
}

function validateDescription(description) {
  // Description: max 2000 characters
  if (!description) return true; // optional
  return description.trim().length <= 2000;
}

function validateAltNames(altNames) {
  // AltNames: max 500 characters
  if (!altNames) return true; // optional
  return altNames.trim().length <= 500;
}

function validateProductId(id) {
  // ID: must be positive integer
  return id && !isNaN(id) && Number(id) > 0 && Number.isInteger(Number(id));
}

function validateSizesArray(sizes) {
  // Sizes: must be non-empty array with valid entries
  if (!Array.isArray(sizes) || sizes.length === 0) {
    return { valid: false, message: "At least one size with stock is required" };
  }

  for (let i = 0; i < sizes.length; i++) {
    const s = sizes[i];
    
    if (!s.size_value || !validateSizeValue(s.size_value)) {
      return { 
        valid: false, 
        message: `Invalid size_value at index ${i}. Must be 1-20 characters` 
      };
    }
    
    if (s.stock === null || s.stock === undefined || !validateStock(s.stock)) {
      return { 
        valid: false, 
        message: `Invalid stock at index ${i}. Must be 0 or positive integer` 
      };
    }
  }

  return { valid: true };
}
