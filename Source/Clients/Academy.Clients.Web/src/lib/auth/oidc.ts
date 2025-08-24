import { UserManager, WebStorageStateStore } from 'oidc-client-ts';
const config = window.appConfig;

const oidcConfig = {
    authority: config.auth.ISSUER,
    client_id: config.auth.CLIENT_ID,    
    metadataUrl: config.auth.DISCOVERY_ENDPOINT,
    redirect_uri: window.location.origin + '/callback',
    silent_redirect_uri: window.location.origin + '/silent-renew.html',
    post_logout_redirect_uri: window.location.origin,
    response_type: 'code',
    scope: 'openid profile email',
    userStore: new WebStorageStateStore({ store: window.localStorage }),
};

export const userManager = new UserManager(oidcConfig);