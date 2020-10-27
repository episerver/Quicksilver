export function setMessage(message: string) {
  return {
    type: 'SET_MESSAGE',
    message,
  } as const;
}

export function setData(data: any) {
  return {
    type: 'SET_DATA',
    payload: data,
  } as const;
}

export type ApplicationActionType = 
| ReturnType<typeof setMessage>
| ReturnType<typeof setData>;
