export interface JwtPayload {
  sub?: string;
  name?: string;
  unique_name?: string;
  email?: string;
  uid?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'?: string;
}
