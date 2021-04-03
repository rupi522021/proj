import React, { Component } from 'react'
import Header from './Header';
import LogInScr from './LogInScr';
import Footer from './Footer';
import ChangePasswordScr from './ChangePasswordScr';
import Content from './Content';
import { MsgContext, apiUrl, fetchCall } from '../consts/MainConst';

export default class MainComponent extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.state = {
      // logedIn: true,
      // logedInUser: this.tmpUser,
      logedIn: false,
      logedInUser: {
        UserName: '',
        IsNewPassword: false
      }
    }
  }
  tmpUser = {
    Active: true,
    Email: null,
    FullName: null,
    IsNewPassword: false,
    IsSaleMenager: false,
    Password: null,
    Permissions: [1, 2, 3, 4, 6],
    PersonId: 0,
    Token: "ttrs50eAgQjrhDjm2MRK9teAUuNTqELfodssxAUbZkrOHgtGOyBNh5m0FgOBcAKS5323KqossplO1Fsn075r3xxjqsdGmJyPQPIDdqvXAIVrKOyzoaBV5SP76ICSDX6Y",
    UserName: "ABROADM",
    UserTypes: [2, 4]
  }
  LogIn = (data) => { this.setState({ logedIn: true, logedInUser: data.User }); }

  LogOut = () => {
    fetchCall('GET', `${apiUrl}/logout/`, '', this.LogOutSuccess, this.LogOutError, this.state.logedInUser.Token, this.state.logedInUser.UserName);
  }

  LogOutSuccess = (data) => {
    this.MessageShow(data.Message, 1);
    this.setState({
      logedIn: false,
      logedInUser: {
        UserName: '',
        IsNewPassword: false
      }
    });
  }

  LogOutError = (error) => {
    this.MessageShow(error.Message, 0);
    this.setState({
      logedIn: false,
      logedInUser: {
        UserName: '',
        IsNewPassword: false
      }
    });
  }

  render() {
    return (
      <div>
        <Header logedIn={this.state.logedIn}
          MustChangePassword={this.state.logedInUser.IsNewPassword}
          userName={this.state.logedInUser.UserName}
          LogOut={this.LogOut} />
        {(this.state.logedIn ?
          (this.state.logedInUser.IsNewPassword ?
            <ChangePasswordScr logedInUser={this.state.logedInUser} LogIn={this.LogIn} /> :
            <Content logedInUser={this.state.logedInUser} />)
          : <LogInScr LogIn={this.LogIn} />)}
        <Footer />
      </div>
    )
  }
}