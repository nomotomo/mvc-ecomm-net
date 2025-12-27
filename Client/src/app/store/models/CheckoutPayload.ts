export interface CheckoutPayload {
  username: string;
  totalPrice: number;
  firstName: string;
  lastName: string;
  emailAddress: string;
  addressLine: string;
  country: string;
  state: string;
  zipCode: string;
  cardHolderName: string;
  cardNumber: string;
  cardExpiration: string;
  cardSecurityNumber: string;
  cardTypeId: number;
}
