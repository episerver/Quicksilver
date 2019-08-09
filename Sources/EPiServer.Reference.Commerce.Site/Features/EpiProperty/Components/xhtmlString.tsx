import React from 'react';
import { IEpiProperty } from '..';

export interface IXhtmlString extends IEpiProperty {
  value: string;
}

export default ({ value }: IXhtmlString) => {
  return value && <div dangerouslySetInnerHTML={{ __html: value }} />;
};
