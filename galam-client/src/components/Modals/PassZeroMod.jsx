import React, { Component } from 'react'
import { FormControl, IconButton, InputAdornment, InputLabel, OutlinedInput } from '@material-ui/core';
import { Visibility, VisibilityOff } from '@material-ui/icons';
import ModalWidow from './ModalWidow';
import { MsgContext, PasswordNotValidMsg, PasswordPattern } from '../../consts/MainConst';

export default class PassZeroMod extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.state = {
      password: '',
      showPassword: false
    }
    this.classes = {
      inputStyle: {
        margin: 10,
        width: 250,
        direction: 'ltr'
      }
    };
  }

  handleClickShowPassword = () => {
    let sh = this.state.showPassword;
    this.setState({ showPassword: !sh });
  };

  handleMouseDownPassword = (e) => { e.preventDefault(); };

  passwordChange = (e) => { this.setState({ password: e.target.value }); };

  validateForm = () => {
    let valid = true;
    let passwordEmptyMsg = "סיסמה ריקה";
    if (this.state.password == "") {
      valid = false;
      this.MessageShow(passwordEmptyMsg, 0);
    }
    else if (!this.state.password.match(PasswordPattern)) {
      valid = false;
      this.MessageShow(PasswordNotValidMsg, 0, 5000);
    }
    return valid;
  }

  onClickSave = () => { if (this.validateForm()) this.props.saveF(this.props.user, this.state.password); }

  render() {
    return (
      <ModalWidow cancelX={this.props.cancelX} header={`איפוס סיסמת משתמש ${this.props.user}`}>
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
        <div className="cntTxt">
          <button className="blueButton" onClick={this.onClickSave}>שמור</button>
          <button className="blueButton" onClick={this.props.cancelX}>בטל</button>
        </div>
      </ModalWidow>
    )
  }
}