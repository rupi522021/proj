import { FormControl, IconButton, InputAdornment, InputLabel, OutlinedInput } from '@material-ui/core';
import { Visibility, VisibilityOff } from '@material-ui/icons';
import React, { Component } from 'react'
import { MsgContext, PasswordNotValidMsg, PasswordPattern, UserNameNotValidMsg, UserNamePattern } from '../../consts/MainConst';
import ModalWidow from './ModalWidow'

export default class AddUserMod extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.state = {
      userName: '',
      password: '',
      personNumber: '',
      showPassword: false,
    }
    this.classes = {
      inputStyle: {
        margin: 10,
        width: 250,
        direction: 'ltr'
      }
    };
  }

  userNameChange = (e) => { this.setState({ userName: e.target.value }) }

  personNumberChange = (e) => { this.setState({ personNumber: e.target.value }) }

  handleClickShowPassword = () => {
    let sh = this.state.showPassword;
    this.setState({ showPassword: !sh });
  };

  handleMouseDownPassword = (e) => { e.preventDefault(); };

  passwordChange = (e) => { this.setState({ password: e.target.value }); };

  validateForm = () => {
    let valid = true;
    let userNameEmptyMsg = "שם משתמש ריק";
    let passwordEmptyMsg = "סיסמה ריקה";
    if (this.state.userName == "") {
      valid = false;
      this.MessageShow(userNameEmptyMsg, 0);
    }
    else if (!this.state.userName.match(UserNamePattern)) {
      valid = false;
      this.MessageShow(UserNameNotValidMsg, 0, 5000);
    }
    this.props.AllUsers.forEach(user => {
      if (user.UserName == this.state.userName.toUpperCase()) {
        valid = false;
        this.MessageShow(`משתמש ${user.UserName} כבר קיים במערכת`, 0, 5000);
      }
    })
    if (this.state.password == "") {
      valid = false;
      this.MessageShow(passwordEmptyMsg, 0);
    }
    else if (!this.state.password.match(PasswordPattern)) {
      valid = false;
      this.MessageShow(PasswordNotValidMsg, 0, 5000);
    }
    let personNumberValid = false;
    if (this.state.personNumber != "") this.props.AllPeople.forEach(item => { if (item == this.state.personNumber) { personNumberValid = true; } })
    else personNumberValid = true;
    if (!personNumberValid) {
      valid = false;
      this.MessageShow("מספר עובד לא קיים", 0, 5000);
    }
    return valid;
  }

  onClickSave = () => {
    if (this.validateForm()) this.props.saveF({
      UserName: this.state.userName.toUpperCase(),
      Password: this.state.password,
      PersonId: Number(this.state.personNumber),
      Permissions: [],
      UserTypes: []
    });
  }

  render() {
    return (
      <div>
        <ModalWidow cancelX={this.props.cancelX} header={`משתמש חדש`}>
          <FormControl variant="outlined" style={this.classes.inputStyle}>
            <InputLabel htmlFor="component-outlined">שם משתמש</InputLabel>
            <OutlinedInput id="component-outlined" value={this.state.userName} onChange={this.userNameChange} label="שם משתמש" />
          </FormControl>
          <br />
          <FormControl variant="outlined" style={this.classes.inputStyle}>
            <InputLabel htmlFor="person-number">מספר עובד</InputLabel>
            <OutlinedInput id="person-number" value={this.state.personNumber} onChange={this.personNumberChange} label="מספר עובד" type="number" />
          </FormControl>
          <br />
          <FormControl variant="outlined" style={this.classes.inputStyle}>
            <InputLabel htmlFor="outlined-adornment-password">סיסמה זמנית</InputLabel>
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
              label="סיסמה זמנית"
            />
          </FormControl>
          <br />
          <div className="cntTxt">
            <button className="blueButton" onClick={this.onClickSave}>המשך</button>
            <button className="blueButton" onClick={this.props.cancelX}>בטל</button>
          </div>
        </ModalWidow>
      </div>
    )
  }
}