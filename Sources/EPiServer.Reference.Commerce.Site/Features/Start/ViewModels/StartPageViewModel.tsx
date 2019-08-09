import EpiProperty from 'features/EpiProperty';
import React from 'react';

export default (props: any) => {
  return (
    <div>
      the start
      <EpiProperty for={props.startPage.testContentArea} />
      <EpiProperty for={props.startPage.mainBody} />
    </div>
  );
};
