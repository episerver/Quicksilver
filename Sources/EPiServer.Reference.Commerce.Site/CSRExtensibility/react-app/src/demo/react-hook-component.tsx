import React, { useState } from 'react';

export const ReactHookComponent = () => {
  const [message, setMessage] = useState('');

  React.useEffect(() => {
    setMessage('This is message.');
  }, [message]);

  return <h1>React hook component: {message}</h1>;
};
