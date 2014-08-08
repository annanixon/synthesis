﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;

public class OGL_RigidNode : RigidNode_Base
{
    private TransMatrix myTrans = new TransMatrix();
    private List<VBOMesh> models = new List<VBOMesh>();

    private float requestedRotation = 0;
    private float requestedTranslation = 0;

    public void loadMeshes(string path)
    {
        BXDAMesh mesh = new BXDAMesh();
        mesh.ReadFromFile(path);
        foreach (BXDAMesh.BXDASubMesh sub in mesh.meshes)
        {
            models.Add(new VBOMesh(sub));
        }
    }

    // A*B*C = C, then B, then A
    float i = 0;
    public void compute()
    {
        i += 0.01f;
        requestedTranslation = (float) Math.Sin(i) * 100;
        requestedRotation = (float) Math.Cos(i);
        myTrans.identity();
        if (GetSkeletalJoint() != null)
        {
            BXDVector3 baseV = null, axis = null;
            float modelTranslation = 0, modelRotation = 0;
            switch (GetSkeletalJoint().GetJointType())
            {
                case SkeletalJointType.ROTATIONAL:
                    RotationalJoint_Base rjb = (RotationalJoint_Base) GetSkeletalJoint();
                    baseV = rjb.basePoint;
                    axis = rjb.axis;
                    requestedTranslation = 0;
                    modelRotation = rjb.currentAngularPosition;
                    break;
                case SkeletalJointType.LINEAR:
                    LinearJoint_Base ljb = (LinearJoint_Base) GetSkeletalJoint();
                    baseV = ljb.basePoint;
                    axis = ljb.axis;
                    requestedRotation = 0;
                    modelTranslation = ljb.currentLinearPosition;
                    break;
                case SkeletalJointType.CYLINDRICAL:
                    CylindricalJoint_Base cjb = (CylindricalJoint_Base) GetSkeletalJoint();
                    baseV = cjb.basePoint;
                    axis = cjb.axis;
                    modelRotation = cjb.currentAngularPosition;
                    modelTranslation = cjb.currentLinearPosition;
                    if (cjb.hasLinearEndLimit)
                        requestedTranslation = Math.Min(requestedTranslation, cjb.linearLimitEnd);
                    if (cjb.hasLinearStartLimit)
                        requestedTranslation = Math.Max(requestedTranslation, cjb.linearLimitStart);
                    break;
            }
            if (GetParent() != null)
            {
                baseV = ((OGL_RigidNode) GetParent()).myTrans.multiply(baseV);
                axis = ((OGL_RigidNode) GetParent()).myTrans.rotate(axis);
            }
            TransMatrix mat = new TransMatrix();

            mat.identity().setTranslation(baseV.x, baseV.y, baseV.z);
            myTrans.multiply(mat);
            mat.identity().setRotation(axis.x, axis.y, axis.z, requestedRotation - modelRotation + (float) Math.PI);
            mat.setTranslation(axis.x * (requestedTranslation - modelTranslation), axis.y * (requestedTranslation - modelTranslation), axis.z * (requestedTranslation - modelTranslation));
            myTrans.multiply(mat);
            mat.identity().setTranslation(-baseV.x, -baseV.y, -baseV.z);
            myTrans.multiply(mat);
        }
        if (GetParent() != null)
        {
            myTrans.multiply(((OGL_RigidNode) GetParent()).myTrans);
        }
        foreach (RigidNode_Base child in children.Values)
        {
            ((OGL_RigidNode) child).compute();
        }
    }

    public void render()
    {
        Gl.glPushMatrix();
        Gl.glMultMatrixf(myTrans.toBuffer());
        foreach (VBOMesh mesh in models)
        {
            mesh.draw();
        }
        Gl.glPopMatrix();
    }

    public override object GetModel()
    {
        return models;
    }
}
