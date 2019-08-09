import AvailableTypes from 'features/Components/availableTypes';
import { ItemCloneUpdate } from 'features/Components/common';
import React from 'react';
import { IEpiProperty } from '..';

export interface IContentArea extends IEpiProperty {
  items?: any[];
}

export default (props: IContentArea) => {
  return (props.items||[]).map((item, index) => {
    const Component = AvailableTypes[item.$component];
    if (!Component) {
      throw Error(`Component (${item.$component}) is not available! `);
    }
    return ItemCloneUpdate(<Component {...item} />, componentProps => ({
      ...componentProps,
      key: componentProps.key || index
    }));
  });
};
