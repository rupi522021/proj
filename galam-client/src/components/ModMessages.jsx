import React, { Component } from 'react'

export default class ModMessages extends Component {
  constructor(props) {
    super(props);
    this.counter = 0;
    this.msgArr = [];
    this.state = {
      Messages: []
    };
  };

  componentDidMount() {
    this.props.ReassignModMessagesCallCh(this.messageShow);
  }

  messageShow = (msg, isSuccessShow, delMilSec = 3000) => {
    this.counter = this.counter + 1;
    let msgId = this.counter;
    this.msgArr = [...this.msgArr, { Message: msg, isSuccess: isSuccessShow, id: msgId }];
    this.setState({ Messages: this.msgArr });
    setTimeout(() => this.messageHide(msgId), delMilSec);
  }

  messageHide = (id) => {
    this.msgArr = this.msgArr.filter(item => item.id != id);
    this.setState({ Messages: this.msgArr });
  }

  render() {
    return (
      <div>
        <div className="modAns">
          {this.state.Messages.map(item => <div className={item.isSuccess == 1 ? 'ansDivSuccess' : 'ansDivError'} key={item.id}>{item.Message}</div>)}
        </div>
      </div>
    )
  }
}