import axios from 'axios';
import React, { useEffect, useState } from 'react';

function MainNavigation() {
  const [data, setData] = useState({});

  useEffect(() => {
    axios.get('/Navigation/Index').then(result => {
      setData(result.data);
    });
  }, []);
  console.log('THE NAVIGATION',data);
  return <div>THE NAVIAGATION - { data && data.startPage && data.startPage.registrationConfirmationMail}</div>;
}

export default MainNavigation;
