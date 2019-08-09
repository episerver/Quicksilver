import React from 'react';
import { IEpiProperty } from '..';
import EpiProperty from 'features/EpiProperty';

export interface IXhtmlString extends IEpiProperty {
  value: string;
}

export default (props) => {
    console.log('THE BLOCK', props);
  return <div>
      The Free Text
        <EpiProperty for={props.mainBody} />
  </div>;
};
