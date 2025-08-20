import NextAuth from "next-auth";
import { NextAuthOptions } from "next-auth";
import FusionAuthProvider from "next-auth/providers/fusionauth";

export const authOptions: NextAuthOptions = {
  providers: [
  FusionAuthProvider({
    id: "fusionauth",
    name: "FusionAuth",
    issuer:  process.env.FUSIONAUTH_ISSUER,
    clientId: process.env.FUSIONAUTH_CLIENT_ID,
    clientSecret: process.env.FUSIONAUTH_SECRET,
    tenantId: process.env.FUSIONAUTH_TENANT_ID // Only required if you're using multi-tenancy
  }),
  ],
  // Add more options as needed
};

export default NextAuth(authOptions);