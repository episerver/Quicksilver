import * as React from 'react';
import { StoreState } from '../store/index';
import { connect } from 'react-redux';

interface Prop {
  Id: number;
  message?: string;
}

const PlaceHolder = (props: Prop) => {
  return (
    <div>
      <h1>Redux container component</h1>
      <h3>Message: {props.message} </h3>
    </div>
  );
};

interface ContainerProps {
  Id: number;
}

const mapStateToProps = (state: StoreState, ownProps: ContainerProps): Prop => {
  return {
    message: state.application.message,
    Id: ownProps.Id
  };
};

export const ReduxContainerComponent = connect(mapStateToProps)(PlaceHolder);