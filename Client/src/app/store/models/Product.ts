import { Brand } from "./Brand";
import { Type } from "./Type";

export interface Product {
  id: string;
  name: string;
  description: string;
  summary: string,
  imageFile: string;
  price: number;
  brands: {
    id: string;
    name: string;
  };
  types: {
    id: string;
    name: string;
  };
}
