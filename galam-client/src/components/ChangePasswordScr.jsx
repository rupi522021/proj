import { FormControl, IconButton, InputAdornment, InputLabel, OutlinedInput } from '@material-ui/core';
import { Visibility, VisibilityOff } from '@material-ui/icons';
import React, { Component } from 'react'
import { MsgContext, apiUrl, fetchCall, PasswordPattern, PasswordNotValidMsg } from '../consts/MainConst';

export default class ChangePasswordScr extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.state = {
      oldPassword: '',
      showOldPassword: false,
      newPassword: '',
      showNewPassword: false,
      newSecondPassword: '',
      showNewSecondPassword: false,
    };
    this.classes = {
      inputStyle: {
        margin: 10,
        width: 250
      }
    };
  }

  handleMouseDownPassword = (e) => { e.preventDefault(); };

  handleClickShowOldPassword = () => {
    let sh = this.state.showOldPassword;
    this.setState({ showOldPassword: !sh });
  };

  oldPasswordChange = (e) => { this.setState({ oldPassword: e.target.value }); };

  handleClickShowNewPassword = () => {
    let sh = this.state.showNewPassword;
    this.setState({ showNewPassword: !sh });
  };

  newPasswordChange = (e) => { this.setState({ newPassword: e.target.value }); };

  handleClickShowNewSecondPassword = () => {
    let sh = this.state.showNewSecondPassword;
    this.setState({ showNewSecondPassword: !sh });
  };

  newSecondPasswordChange = (e) => { this.setState({ newSecondPassword: e.target.value }); };

  validateForm = () => {
    let valid = true;
    let passwordEmptyMsg = "סיסמה חדשה ריקה";
    if (this.state.newPassword == "") {
      valid = false;
      this.MessageShow(passwordEmptyMsg, 0);
    }
    else if (!this.state.newPassword.match(PasswordPattern)) {
      valid = false;
      this.MessageShow(PasswordNotValidMsg, 0, 5000);
    }
    if (this.state.newPassword != this.state.newSecondPassword) {
      valid = false;
      this.MessageShow("סיסמה חדשה לא זהה לסיסמה מאומתת", 0, 5000);
    }
    return valid;
  }

  onPressSave = () => {
    if (this.validateForm()) {
      let api = `${apiUrl}/changePassword/?UserName=${this.props.logedInUser.UserName}&OldPassword=${this.state.oldPassword}&NewPassword=${this.state.newPassword}`;
      fetchCall('POST', api, '', this.props.LogIn, this.onPressSaveError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
    }
  }

  onPressSaveError = (error) => { this.MessageShow(error.Message, 0); }

  render() {
    return (
      <div className="main contentWoMenu logIn" dir="ltr">
        <div className="paddingVh"></div>
        <div><h2 style={{ direction: "rtl" }}>שינוי סיסמת משתמש {this.props.logedInUser.UserName}</h2></div>
        <FormControl variant="outlined" style={this.classes.inputStyle}>
          <InputLabel htmlFor="outlined-old-password">סיסמה ישנה</InputLabel>
          <OutlinedInput
            id="outlined-old-password"
            type={this.state.showOldPassword ? 'text' : 'password'}
            value={this.state.oldPassword}
            onChange={this.oldPasswordChange}
            endAdornment={
              <InputAdornment position="end">
                <IconButton
                  aria-label="toggle password visibility"
                  onClick={this.handleClickShowOldPassword}
                  onMouseDown={this.handleMouseDownPassword}
                  edge="end"
                >
                  {this.state.showOldPassword ? <Visibility /> : <VisibilityOff />}
                </IconButton>
              </InputAdornment>
            }
            label="סיסמה ישנה"
          />
        </FormControl>
        <br />
        <FormControl variant="outlined" style={this.classes.inputStyle}>
          <InputLabel htmlFor="outlined-new-password">סיסמה חדשה</InputLabel>
          <OutlinedInput
            id="outlined-new-password"
            type={this.state.showNewPassword ? 'text' : 'password'}
            value={this.state.newPassword}
            onChange={this.newPasswordChange}
            endAdornment={
              <InputAdornment position="end">
                <IconButton
                  aria-label="toggle password visibility"
                  onClick={this.handleClickShowNewPassword}
                  onMouseDown={this.handleMouseDownPassword}
                  edge="end"
                >
                  {this.state.showNewPassword ? <Visibility /> : <VisibilityOff />}
                </IconButton>
              </InputAdornment>
            }
            label="סיסמה חדשה"
          />
        </FormControl>
        <br />
        <FormControl variant="outlined" style={this.classes.inputStyle}>
          <InputLabel htmlFor="outlined-newSecond-password">אימות סיסמה</InputLabel>
          <OutlinedInput
            id="outlined-newSecond-password"
            type={this.state.showNewSecondPassword ? 'text' : 'password'}
            value={this.state.newSecondPassword}
            onChange={this.newSecondPasswordChange}
            endAdornment={
              <InputAdornment position="end">
                <IconButton
                  aria-label="toggle password visibility"
                  onClick={this.handleClickShowNewSecondPassword}
                  onMouseDown={this.handleMouseDownPassword}
                  edge="end"
                >
                  {this.state.showNewSecondPassword ? <Visibility /> : <VisibilityOff />}
                </IconButton>
              </InputAdornment>
            }
            label="אימות סיסמה"
          />
        </FormControl>
        <br />
        <button className="blueButton" onClick={this.onPressSave}>שמור</button>
      </div>
    )
  }
}