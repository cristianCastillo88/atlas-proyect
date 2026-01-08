import { persistentAtom } from '@nanostores/persistent';

export interface UserData {
  token: string;
  role: string;
  name: string;
  negocioId: string;
}

export const $userStore = persistentAtom<UserData | null>('userStore', null, {
  encode: JSON.stringify,
  decode: JSON.parse,
});

export function setLoginData(data: UserData) {
  $userStore.set(data);
}

export function logout() {
  $userStore.set(null);
}

export function isAuthenticated(): boolean {
  const user = $userStore.get();
  return user !== null && !!user.token;
}