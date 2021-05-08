import React, { Component } from 'react'
import { MsgContext, apiUrl, fetchCall } from '../../consts/MainConst';
import AddUserMod from '../Modals/AddUserMod';
import PassZeroMod from '../Modals/PassZeroMod';
import SetRoolMod from '../Modals/SetRoolMod';
import VsbTable from '../VsbTable';

export default class UserManagement extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.data = [];
    this.columns = [
      { name: "User", data: "UserName" },
      { name: "Name", data: "FullName" },
      { name: "Email", data: "Email" },
      { name: "", data: "", sortable: false, render: this.iconsRender },
    ];
    this.state = {
      modalToShow: "",
    }
    this.classes = {
      inputStyle: {
        margin: 10,
        width: 250,
        direction: 'ltr'
      }
    };
  }

  componentDidMount() { this.getAllUsers(); }

  getAllUsers = () => { fetchCall('GET', `${apiUrl}/users/`, '', this.getAllUsersSuccess, this.getAllUsersError, this.props.logedInUser.Token, this.props.logedInUser.UserName); }

  getAllUsersSuccess = (data) => {
    this.data = data.Users;
    this.setState({});
  }

  getAllUsersError = (error) => { this.MessageShow(error.Message, 0); }

  iconsRender = (row, data, index) => {
    return (
      [<img data-user={row.UserName} src="img/Options.png" className="inLineImg" onClick={this.setRool} key={`img ${index} setRool`} />,
      row.Active ?
        <img data-user={row.UserName} src="img/LockU.png" className="inLineImg" onClick={this.lockUser} key={`img ${index} lockUser`} />
        : <img data-user={row.UserName} src="img/LockL.png" className="inLineImg noClick" key={`img ${index} lockUser`} />,
      row.Active ?
        <img data-user={row.UserName} src="img/UnlockU.png" className="inLineImg noClick" key={`img ${index} unLockUser`} />
        : <img data-user={row.UserName} src="img/UnlockL.png" className="inLineImg" onClick={this.unLockUser} key={`img ${index} unLockUser`} />,
      <img data-user={row.UserName} src="img/key.png" className="inLineImg" onClick={this.passZero} key={`img ${index} passZero`} />]
    );
  }

  setRool = (e) => {
    this.data.forEach(item => { if (e.currentTarget.dataset.user == item.UserName) this.userSetRoolMod = item });
    fetchCall('GET', `${apiUrl}/roolGet/`, '', this.setRoolSuccess, this.setRoolError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  setRoolSuccess = (data) => {
    this.PermissionTypes = data.Rools.PermissionTypes;
    this.UserTypes = data.Rools.UserTypes;
    this.setState({ modalToShow: "SetRoolMod", userSetRoolMod: this.userSetRoolMod });
  }

  setRoolError = (error) => { this.MessageShow(`${error.Message}. לא ניתן לבצע עדכון הרשאות.`, 0); }

  setRoolSave = (user) => {
    fetchCall('POST', `${apiUrl}/roolPost/`, user, this.setRoolSaveSuccess, this.setRoolSaveError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  setRoolSaveSuccess = (data) => {
    this.MessageShow(data.Message, 1);
    this.data.forEach(item => {
      if (data.user.UserName == item.UserName) {
        item.Permissions = data.user.Permissions;
        item.UserTypes = data.user.UserTypes;
      }
    });
    this.hideModal();
  }

  setRoolSaveError = (error) => { this.MessageShow(error.Message, 0); }

  lockUser = (e) => {
    fetchCall('POST', `${apiUrl}/lockUser/`, e.currentTarget.dataset.user, this.lockUserSuccess, this.lockUserError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  lockUserSuccess = (data) => {
    this.data.forEach(item => { if (item.UserName == data.UserName) item.Active = false });
    this.MessageShow(data.Message, 1);
    this.setState({});
  }

  lockUserError = (error) => { this.MessageShow(error.Message, 0); }

  unLockUser = (e) => {
    fetchCall('POST', `${apiUrl}/unLockUser/`, e.currentTarget.dataset.user, this.unLockUserSuccess, this.unLockUserError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  unLockUserSuccess = (data) => {
    this.data.forEach(item => { if (item.UserName == data.UserName) item.Active = true });
    this.MessageShow(data.Message, 1);
    this.setState({});
  }

  unLockUserError = (error) => { this.MessageShow(error.Message, 0); }

  passZero = (e) => { this.setState({ modalToShow: "PassZeroMod", userPassZeroMod: e.currentTarget.dataset.user }); }

  passZeroSave = (user, pass) => {
    fetchCall('POST', `${apiUrl}/passZero/`, { UserName: user, Password: pass }, this.passZeroSaveSuccess, this.passZeroSaveError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  passZeroSaveSuccess = (data) => {
    this.MessageShow(data.Message, 1);
    this.setState({ modalToShow: "" });
  }

  passZeroSaveError = (error) => { this.MessageShow(error.Message, 0); }

  addUser = () => {
    fetchCall('GET', `${apiUrl}/peopleGet/`, '', this.addUserSuccess, this.addUserError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  addUserSuccess = (data) => {
    this.setState({ modalToShow: "AddUserMod", allPeople: data.People });
  }

  addUserError = (error) => { this.MessageShow(error.Message, 0); }

  addUserSave = (user) => {
    this.userSetRoolMod = user;
    fetchCall('GET', `${apiUrl}/roolGet/`, '', this.addUserSaveSuccess, this.addUserSaveError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  addUserSaveSuccess = (data) => {
    this.PermissionTypes = data.Rools.PermissionTypes;
    this.UserTypes = data.Rools.UserTypes;
    this.setState({ modalToShow: "AddUserSetRoolMod", userSetRoolMod: this.userSetRoolMod });
  }

  addUserSetRoolSave = (user) => {
    fetchCall('PUT', `${apiUrl}/addUser/`, user, this.addUserSetRoolSaveSuccess, this.addUserSetRoolSaveError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  addUserSetRoolSaveSuccess = (data) => {
    this.MessageShow(data.Message, 1);
    this.data.push(data.user);
    this.hideModal();
  }

  addUserSetRoolSaveError = (error) => { this.MessageShow(error.Message, 0); }


  addUserSaveError = (error) => { this.MessageShow(`${error.Message}. לא ניתן לבצע עדכון הרשאות.`, 0); }

  refresh = () => { this.getAllUsers(); }

  hideModal = () => { this.setState({ modalToShow: "" }); }

  render() {
    return (
      <div>
        <div className="divToolbar">
          <div></div>
          <div className="divToolbarImg">
            <img src="img/plus.png" onClick={this.addUser} />
            <img src="img/Refresh.png" onClick={this.refresh} />
          </div>
          <div></div>
        </div>
        <VsbTable
          sortable={true}
          data={this.data}
          columns={this.columns}
          divClass="heightWithTwoMenu"
        />
        {this.state.modalToShow == "PassZeroMod" ? <PassZeroMod user={this.state.userPassZeroMod} cancelX={this.hideModal} saveF={this.passZeroSave} /> : ""}
        {this.state.modalToShow == "SetRoolMod" ?
          <SetRoolMod user={this.state.userSetRoolMod} cancelX={this.hideModal} saveF={this.setRoolSave}
            AllUserTypes={this.UserTypes} AllPermissionTypes={this.PermissionTypes} /> : ""}
        {this.state.modalToShow == "AddUserMod" ? <AddUserMod cancelX={this.hideModal} AllUsers={this.data} AllPeople={this.state.allPeople} saveF={this.addUserSave} /> : ""}
        {this.state.modalToShow == "AddUserSetRoolMod" ?
          <SetRoolMod user={this.state.userSetRoolMod} cancelX={this.hideModal}
            saveF={this.addUserSetRoolSave} AllUserTypes={this.UserTypes} AllPermissionTypes={this.PermissionTypes} /> : ""}
      </div>
    )
  }
}