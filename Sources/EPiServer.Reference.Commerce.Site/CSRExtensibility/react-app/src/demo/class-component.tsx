import * as React from 'react';
import { HttpRequest } from '@episerver/csr-extension';

interface OrderDetailProps {
  Id: number
}

export class OrderDetailTab extends React.Component<OrderDetailProps> {
  state = {
    message: '',
  };

  async componentDidMount() {
    let message = (await this.getData()).data;
    this.setState({
      message: message,
    });
  }

  getData = async () => {
    return await HttpRequest.get('csr-demo/getData');
  };

  render() {
    return (
      <div>
        <h1>Class component</h1>
        <h3>Message from remote: {this.state.message}</h3>
      </div>
    );
  }
}