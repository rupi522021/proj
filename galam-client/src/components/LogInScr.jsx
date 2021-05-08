import { FormControl, IconButton, InputAdornment, InputLabel, OutlinedInput } from '@material-ui/core';
import { Visibility, VisibilityOff } from '@material-ui/icons';
import React, { Component } from 'react'
import { MsgContext, apiUrl, fetchCall } from '../consts/MainConst';

export default class LogInScr extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.state = {
      logedIn: false,
      userName: '',
      MustChangePassword: false,
      password: '',
      showPassword: false,
      saveCheckbox: false
    };
    this.classes = {
      inputStyle: {
        margin: 10,
        width: 250
      }
    };
  }

  UserNameChange = (e) => { this.setState({ userName: e.target.value }) }

  handleClickShowPassword = () => {
    let sh = this.state.showPassword;
    this.setState({ showPassword: !sh });
  };

  handleMouseDownPassword = (e) => { e.preventDefault(); };

  passwordChange = (e) => { this.setState({ password: e.target.value }); };

  saveCheckboxChange = (e) => { this.setState({ saveCheckbox: e.target.checked }); };

  onPressLogInError = (error) => { this.MessageShow(error.Message, 0); }

  onPressLogIn = () => {
    if (this.validateForm()) {
      let api = `${apiUrl}/login/?UserName=${this.state.userName}&Password=${this.state.password}`;
      fetchCall('GET', api, '', this.props.LogIn, this.onPressLogInError);
    }
  }

  validateForm = () => {
    let valid = true;
    let userNameEmptyMsg = "שם משתמש ריק";
    let passwordEmptyMsg = "סיסמה ריקה";
    if (this.state.userName == "") {
      valid = false;
      this.MessageShow(userNameEmptyMsg, 0);
    }
    if (this.state.password == "") {
      valid = false;
      this.MessageShow(passwordEmptyMsg, 0);
    }
    return valid;
  }

  render() {
    return (
      <div className="main contentWoMenu logIn" dir="ltr">
        <div className="paddingVh"></div>
        <FormControl variant="outlined" style={this.classes.inputStyle}>
          <InputLabel htmlFor="component-outlined">שם משתמש</InputLabel>
          <OutlinedInput id="component-outlined" value={this.state.userName} onChange={this.UserNameChange} label="שם משתמש" />
        </FormControl>
        <br />
        <FormControl variant="outlined" style={this.classes.inputStyle}>
          <InputLabel htmlFor="outlined-adornment-password">סיסמה</InputLabel>
          <OutlinedInput
            id="outlined-adornment-password"
            type={this.state.showPassword ? 'text' : 'password'}
            value={this.state.password}
            onChange={this.passwordChange}
            endAdornment={
              <InputAdornment position="end">
                <IconButton
                  aria-label="toggle password visibility"
                  onClick={this.handleClickShowPassword}
                  onMouseDown={this.handleMouseDownPassword}
                  edge="end"
                >
                  {this.state.showPassword ? <Visibility /> : <VisibilityOff />}
                </IconButton>
              </InputAdornment>
            }
            label="סיסמה"
          />
        </FormControl>
        <br />
        <button className="blueButton" onClick={this.onPressLogIn}>כניסה</button>
      </div>
    )
  }
}